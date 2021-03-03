using BooksCRUD.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksCRUD.Data.Services
{
    public interface IBookData
    {
        IEnumerable<Book> GetAll();
        Book GetById(int id);

        void Update(Book book);
        void AddBook(Book book);
        void DeleteBook(int id);
    }
}
