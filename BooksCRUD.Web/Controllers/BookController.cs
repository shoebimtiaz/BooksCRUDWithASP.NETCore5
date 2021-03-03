using BooksCRUD.Data.Models;
using BooksCRUD.Data.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksCRUD.Web.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookData _bookData;

        public BookController(IBookData bookData)
        {
            _bookData = bookData;
        }

        public IActionResult Index()
        {
            var books = _bookData.GetAll();
            return View(books);
        }

        public IActionResult Details(int id)
        {
            var book = _bookData.GetById(id);
            if(book == null)
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
        public IActionResult Edit(Book book)
        {
            if (ModelState.IsValid)
            {
                _bookData.Update(book);
                return RedirectToAction("Details", new { id = book.Id });
            }
            return View(book);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Book book)
        {
            if (ModelState.IsValid)
            {
                _bookData.AddBook(book);
                TempData["Message"] = "You have added a new book";
                return RedirectToAction("Index");
            }
            return View();
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
