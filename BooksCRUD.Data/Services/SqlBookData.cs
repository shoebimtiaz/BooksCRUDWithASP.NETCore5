
using BooksCRUD.Data.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;

namespace BooksCRUD.Data.Services
{
    public class SqlBookData : IBookData
    {
        private const string BookListCacheKey = "books:all";
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        private readonly ILogger<SqlBookData> _logger;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        public SqlBookData(IConfiguration configuration, IDistributedCache cache, ILogger<SqlBookData> logger)
        {
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
            _cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };
        }

        private static string GetBookCacheKey(int id) => $"book:{id}";

        private void InvalidateCache(int id)
        {
            _logger.LogDebug("Invalidating cache for book {BookId} and book list", id);
            _cache.Remove(GetBookCacheKey(id));
            _cache.Remove(BookListCacheKey);
        }

        public void AddBook(Book book)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("BooksDb")))
                {
                    using (var command = new SqlCommand("AddBookStoredProcedure", connection))
                    {
                        command.Parameters.Add(new SqlParameter("@BookName", book.Name));
                        command.Parameters.Add(new SqlParameter("@Author", book.Author));
                        command.Parameters.Add(new SqlParameter("@Publisher", book.Publisher));
                        command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@Id",
                            Value = book.Id,
                            IsNullable = false,
                            DbType = DbType.Int32,
                            Direction = ParameterDirection.Output
                        });
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        command.ExecuteNonQuery();

                        book.Id = Convert.ToInt32(command.Parameters["@Id"].Value);
                        InvalidateCache(book.Id);
                        _logger.LogInformation(
                            "Book created in database with {BookId}: {BookName} by {Author}, published by {Publisher}",
                            book.Id, book.Name, book.Author, book.Publisher);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create book {BookName} by {Author}", book.Name, book.Author);
            }
        }

        public void DeleteBook(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("BooksDb")))
                {
                    using (var command = new SqlCommand("DeleteBookStoredProcedure", connection))
                    {
                        command.Parameters.Add(new SqlParameter("@Id", id));
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        command.ExecuteNonQuery();
                        InvalidateCache(id);
                        _logger.LogInformation("Book {BookId} deleted from database", id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete book {BookId}", id);
            }
        }

        public Book GetById(int id)
        {
            try
            {
                var cachedBook = _cache.GetString(GetBookCacheKey(id));
                if (!string.IsNullOrEmpty(cachedBook))
                {
                    _logger.LogDebug("Cache hit for book {BookId}", id);
                    return JsonSerializer.Deserialize<Book>(cachedBook, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogDebug("Cache miss for book {BookId}, querying database", id);

                using (var connection = new SqlConnection(_configuration.GetConnectionString("BooksDb")))
                {
                    using (var command = new SqlCommand("GetBookByIdStoredProcedure", connection))
                    {
                        command.Parameters.Add(new SqlParameter("@Id", id));
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        using (SqlDataReader dr = command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            if (dr.Read())
                            {
                                var book = new Book
                                {
                                    Id = Convert.ToInt32(dr["Id"].ToString()),
                                    Name = dr["BookName"].ToString(),
                                    Author = dr["Author"].ToString(),
                                    Publisher = dr["Publisher"].ToString()
                                };

                                _cache.SetString(GetBookCacheKey(id), JsonSerializer.Serialize(book), _cacheOptions);
                                _logger.LogDebug("Book {BookId} cached after database lookup", id);
                                return book;
                            }
                        }
                    }
                }

                _logger.LogWarning("Book {BookId} not found in database", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve book {BookId}", id);
            }

            return new Book();
        }

        public IEnumerable<Book> GetAll()
        {
            try
            {
                var cachedBooks = _cache.GetString(BookListCacheKey);
                if (!string.IsNullOrEmpty(cachedBooks))
                {
                    var cached = JsonSerializer.Deserialize<List<Book>>(cachedBooks, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    _logger.LogDebug("Cache hit for book list ({BookCount} books)", cached.Count);
                    return cached.OrderBy(book => book.Author);
                }

                _logger.LogDebug("Cache miss for book list, querying database");

                var bookList = new List<Book>();
                using (var connection = new SqlConnection(_configuration.GetConnectionString("BooksDb")))
                {
                    using (var command = new SqlCommand("GetBooksStoredProcedure", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        using (SqlDataReader dr = command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (dr.Read())
                            {
                                var book = new Book()
                                {
                                    Id = Convert.ToInt32(dr["Id"].ToString()),
                                    Name = dr["BookName"].ToString(),
                                    Author = dr["Author"].ToString(),
                                    Publisher = dr["Publisher"].ToString()
                                };
                                bookList.Add(book);
                            }
                        }
                    }
                }

                var orderedBookList = bookList.OrderBy(book => book.Author).ToList();
                _cache.SetString(BookListCacheKey, JsonSerializer.Serialize(orderedBookList), _cacheOptions);
                _logger.LogDebug("Book list cached after database lookup ({BookCount} books)", orderedBookList.Count);
                return orderedBookList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve book list");
            }

            return new List<Book>();
        }

        public void Update(Book book)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("BooksDb")))
                {
                    using (var command = new SqlCommand("UpdateBookStoredProcedure", connection))
                    {
                        command.Parameters.Add(new SqlParameter("@Id", book.Id));
                        command.Parameters.Add(new SqlParameter("@BookName", book.Name));
                        command.Parameters.Add(new SqlParameter("@Author", book.Author));
                        command.Parameters.Add(new SqlParameter("@Publisher", book.Publisher));
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        command.ExecuteNonQuery();
                        InvalidateCache(book.Id);
                        _logger.LogInformation(
                            "Book {BookId} updated in database: {BookName} by {Author}, published by {Publisher}",
                            book.Id, book.Name, book.Author, book.Publisher);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update book {BookId}", book.Id);
            }
        }
    }
}
