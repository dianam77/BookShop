﻿using Core.BookService;
using Microsoft.AspNetCore.Mvc;

namespace Bookshop.Controllers
{
    public class BookController : Controller
    {
        private readonly BookService _bookService;
        public BookController(BookService bookService)
        {
            _bookService = bookService;
        }
        

        public async Task<IActionResult> Index(int id)
        {
            var book = await _bookService.GetBookById(id);
            return View(book);
        }

        public async Task<IActionResult> BookList(int page=1, int pageSize = 4, string search= null)
        {
            var data = await _bookService.GetBookPagination(page, pageSize, search);
            return View(data);
        }

        public async Task<IActionResult> GetBook (int id)
        {
            var book = await _bookService.GetBookById(id);
            return PartialView(book);
        }
    }
}
