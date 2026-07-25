
using BooksCRUD.Data.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BooksCRUD.Data.Services
{
    public class SqlBookData : IBookData
    {
        private const string BookListCacheKey = "books:all";
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        public SqlBookData(IConfiguration configuration, IDistributedCache cache)
        {
            _configuration = configuration;
            _cache = cache;
            _cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };
        }

        private static string GetBookCacheKey(int id) => $"book:{id}";

        private void InvalidateCache(int id)
        {
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
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public Book GetById(int id)
        {
            try
            {
                var cachedBook = _cache.GetString(GetBookCacheKey(id));
                if (!string.IsNullOrEmpty(cachedBook))
                {
                    return JsonSerializer.Deserialize<Book>(cachedBook, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

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
                                return book;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
                    return JsonSerializer.Deserialize<List<Book>>(cachedBooks, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }).OrderBy(book => book.Author);
                }

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
                return orderedBookList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }
        }
    }

}

