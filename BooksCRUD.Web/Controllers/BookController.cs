using BooksCRUD.Data.Models;
using BooksCRUD.Data.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BooksCRUD.Web.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookData _bookData;
        private readonly ILogger<BookController> _logger;

        public BookController(IBookData bookData, ILogger<BookController> logger)
        {
            _bookData = bookData;
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Fetching all books");
            var books = _bookData.GetAll();
            _logger.LogInformation("Retrieved {BookCount} books", books.Count());
            return View(books);
        }

        public IActionResult Details(int id)
        {
            _logger.LogInformation("Fetching book details for {BookId}", id);
            var book = _bookData.GetById(id);
            if (book == null)
            {
                _logger.LogWarning("Book {BookId} not found", id);
                return View("NotFound");
            }
            _logger.LogInformation(
                "Returning details for book {BookId}: {BookName} by {Author}",
                book.Id, book.Name, book.Author);
            return View(book);
        }

        public IActionResult Edit(int id)
        {
            _logger.LogInformation("Loading edit form for book {BookId}", id);
            var book = _bookData.GetById(id);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Book book)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation(
                    "Updating book {BookId}: {BookName} by {Author}, published by {Publisher}",
                    book.Id, book.Name, book.Author, book.Publisher);
                _bookData.Update(book);
                return RedirectToAction("Details", new { id = book.Id });
            }
            LogValidationErrors("Edit", book.Id);
            return View(book);
        }

        [HttpGet]
        public IActionResult Create()
        {
            _logger.LogInformation("Loading create book form");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Book book)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation(
                    "Creating book: {BookName} by {Author}, published by {Publisher}",
                    book.Name, book.Author, book.Publisher);
                _bookData.AddBook(book);
                _logger.LogInformation("Book created with assigned {BookId}", book.Id);
                TempData["Message"] = "You have added a new book";
                return RedirectToAction("Index");
            }
            LogValidationErrors("Create", book.Id);
            return View();
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation("Loading delete confirmation for book {BookId}", id);
            var model = _bookData.GetById(id);
            if (model.Id == 0)
            {
                _logger.LogWarning("Book {BookId} not found for deletion", id);
                return View("NotFound");
            }
            _logger.LogInformation(
                "Confirming deletion of book {BookId}: {BookName} by {Author}",
                model.Id, model.Name, model.Author);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, IFormCollection formCollection)
        {
            var book = _bookData.GetById(id);
            _logger.LogInformation(
                "Deleting book {BookId}: {BookName} by {Author}",
                id, book.Name, book.Author);
            _bookData.DeleteBook(id);
            return RedirectToAction("Index");
        }

        private void LogValidationErrors(string action, int bookId = 0)
        {
            var errors = ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => $"{e.Key}: {string.Join(", ", e.Value.Errors.Select(err => err.ErrorMessage))}");

            _logger.LogWarning(
                "{Action} validation failed for book {BookId}: {ValidationErrors}",
                action, bookId, string.Join("; ", errors));
        }
    }
}
