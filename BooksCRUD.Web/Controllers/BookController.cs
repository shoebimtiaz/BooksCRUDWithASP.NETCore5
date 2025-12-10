using System.Threading.Tasks;
using BooksCRUD.Data.Models;
using BooksCRUD.Data.Services;
using BooksCRUD.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BooksCRUD.Web.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookData _bookData;
        private readonly IBlobService _blobService;

        public BookController(IBookData bookData, IBlobService blobService)
        {
            _bookData = bookData;
            _blobService = blobService;
        }

        public IActionResult Index()
        {
            var books = _bookData.GetAll();
            return View(books);
        }

        public IActionResult Details(int id)
        {
            var book = _bookData.GetById(id);
            if (book == null)
            {
                return View("NotFound");
            }
            return View(book);
        }

        public IActionResult Edit(int id)
        {
            var book = _bookData.GetById(id);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var book = _bookData.GetById(id);
                if (book == null) return View("NotFound");

                book.Name = model.Name;
                book.Author = model.Author;
                book.Publisher = model.Publisher;

                if (model.Image != null && model.Image.Length > 0)
                {
                     using var stream = model.Image.OpenReadStream();
                    // Optionally delete old image from Blob Storage first
                    book.ImageUrl = await _blobService.UploadFileAsync(stream, model.Image.FileName);
                }

                _bookData.Update(book);
                return RedirectToAction("Details", new { id = book.Id });
            }
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? imageUrl = null;

                if (model.Image != null && model.Image.Length > 0)
                {
                     using var stream = model.Image.OpenReadStream();
                    // Upload the image to Blob Storage and get URL
                    imageUrl = await _blobService.UploadFileAsync(stream, model.Image.FileName);
                }

                var book = new Book
                {
                    Name = model.Name,
                    Author = model.Author,
                    Publisher = model.Publisher,
                    ImageUrl = imageUrl
                };

                _bookData.AddBook(book);
                TempData["Message"] = "You have added a new book";
                return RedirectToAction("Index");
            }
            return View(model);
        }


        [HttpGet]
        public IActionResult Delete(int id)
        {
            var model = _bookData.GetById(id);
            if (model.Id == 0)
            {
                return View("NotFound");
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, IFormCollection formCollection)
        {
            _bookData.DeleteBook(id);
            return RedirectToAction("Index");
        }
    }
}
