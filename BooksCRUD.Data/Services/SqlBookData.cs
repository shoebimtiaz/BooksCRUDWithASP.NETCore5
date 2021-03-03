
using BooksCRUD.Data.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksCRUD.Data.Services
{
    public class SqlBookData : IBookData
    {
        private readonly IConfiguration _configuration;

        public SqlBookData(IConfiguration configuration)
        {
            _configuration = configuration;
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
            var book = new Book();
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("BooksDb")))
                {
                    using (var command = new SqlCommand("GetBookByIdStoredProcedure", connection))
                    {
                        command.Parameters.Add(new SqlParameter("@Id", id));
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        using (SqlDataReader dr = command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (dr.Read())
                            {
                                book.Id = Convert.ToInt32(dr["Id"].ToString());
                                book.Name = dr["BookName"].ToString();
                                book.Author = dr["Author"].ToString();
                                book.Publisher = dr["Publisher"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return book;
        }

        public IEnumerable<Book> GetAll()
        {
            var bookList = new List<Book>();
            try
            {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return bookList.OrderBy(book => book.Author);
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

