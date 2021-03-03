using BooksCRUD.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksCRUD.Data.Services
{
    public class InMemoryBookData : IBookData
    {
        List<Book> bookList;
        public InMemoryBookData()
        {
            bookList = new List<Book>
            {
                new Book { Id = 1, Name = "Gun Island", Author = "Amitav Ghosh", Publisher = "DoubleDay"},
                new Book { Id = 2, Name = "Amnesty", Author = "Aravind Adiga", Publisher = "Picador"},
                new Book { Id = 3, Name = "Sleeping on Jupiter", Author = "Anuradha Roy", Publisher = "Ravi Dayal"}

            };
        }

        public void AddBook(Book newBook)
        {
            bookList.Add(newBook);
            newBook.Id = bookList.Max(b => b.Id) + 1;
            
        }

        public void DeleteBook(int id)
        {
            var book = bookList.FirstOrDefault(b => b.Id == id);
            bookList.Remove(book);
        }

        public IEnumerable<Book> GetAll()
        {
            return from b in bookList
                   orderby b.Name
                   select b;
        }

        public Book GetById(int id)
        {
            return bookList.FirstOrDefault(b => b.Id == id);
        }

        public void Update(Book updatedBook)
        {
            var book = bookList.FirstOrDefault(b => b.Id == updatedBook.Id);
            if(book != null)
            {
                book.Name = updatedBook.Name;
                book.Author = updatedBook.Author;
                book.Publisher = updatedBook.Publisher;
            }
           
        }

    }
}
