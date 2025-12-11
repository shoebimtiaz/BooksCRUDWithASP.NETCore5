using System;
using System.Threading.Tasks;
using BooksCRUD.Data.Models;
using BooksCRUD.Data.Services;
using BooksCRUD.Data.Utilities;
using BooksCRUD.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BooksCRUD.Web.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookData _bookData;
        private readonly IBlobService _blobService;
        private readonly IBookLogService _logService;

        public BookController(IBookData bookData, IBlobService blobService, IBookLogService logService)
        {
            _bookData = bookData;
            _blobService = blobService;
            _logService = logService;
        }

        public IActionResult Index()
        {
            var books = _bookData.GetAll();
            return View(books);
        }

        public IActionResult Details(int id)
        {
            var book = _bookData.GetById(id);
            if (book == null) return View("NotFound");

            return View(book);
        }

        public IActionResult Edit(int id)
        {
            var book = _bookData.GetById(id);
            if (book == null) return View("NotFound");

            var vm = new BookCreateViewModel
            {
                Name = book.Name,
                Author = book.Author,
                Publisher = book.Publisher,
                ExistingImage = BookImageHelper.GetImageUrl(book.ImageBlobName, book.ImageUpdatedAt)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var book = _bookData.GetById(id);
            if (book == null) return View("NotFound");

            book.Name = model.Name;
            book.Author = model.Author;
            book.Publisher = model.Publisher;

            if (model.Image != null && model.Image.Length > 0)
            {
                if (!string.IsNullOrEmpty(book.ImageBlobName))
                    await _blobService.DeleteFileAsync(book.ImageBlobName);

                using var stream = model.Image.OpenReadStream();
                var blobName = await _blobService.UploadFileAsync(stream, model.Image.FileName);
                book.ImageBlobName = blobName;
                book.ImageUpdatedAt = DateTime.UtcNow;
            }

            _bookData.Update(book);

            await _logService.LogAsync(new BookLog
            {
                BookId = book.Id.ToString(),
                Action = "Edit",
                Details = $"Book '{book.Name}' edited"
            });

            return RedirectToAction("Details", new { id = book.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string? blobName = null;

            if (model.Image != null && model.Image.Length > 0)
            {
                using var stream = model.Image.OpenReadStream();
                blobName = await _blobService.UploadFileAsync(stream, model.Image.FileName);
            }

            var book = new Book
            {
                Name = model.Name,
                Author = model.Author,
                Publisher = model.Publisher,
                ImageBlobName = blobName,
                ImageUpdatedAt = DateTime.UtcNow
            };

            _bookData.AddBook(book);

            await _logService.LogAsync(new BookLog
            {
                BookId = book.Id.ToString(),
                Action = "Create",
                Details = $"Book '{book.Name}' created"
            });

            TempData["Message"] = "You have added a new book";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var book = _bookData.GetById(id);
            if (book == null) return View("NotFound");

            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection _)
        {
            var book = _bookData.GetById(id);
            if (book == null) return View("NotFound");

            if (!string.IsNullOrEmpty(book.ImageBlobName))
                await _blobService.DeleteFileAsync(book.ImageBlobName);

            _bookData.DeleteBook(id);

            await _logService.LogAsync(new BookLog
            {
                BookId = book.Id.ToString(),
                Action = "Delete",
                Details = $"Book '{book.Name}' deleted"
            });

            return RedirectToAction("Index");
        }
    }
}
