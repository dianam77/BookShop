using Bookshop.Models;
using Core.BookService;
using Core.ServiceFile;
using Microsoft.AspNetCore.Mvc;

namespace Bookshop.Controllers
{
    public class BookController : Controller
    {
        private readonly BookService _bookService;
        private readonly IFileService _fileService;

        public BookController(BookService bookService, IFileService fileService)
        {
            _bookService = bookService;
            _fileService = fileService; // Inject the file service
        }

        public async Task<IActionResult> Index(int id)
        {
            // Get the current book by ID
            var book = await _bookService.GetBookById(id);
            if (book == null)
            {
                return NotFound("Book not found.");
            }

            // Get other books by the same author (excluding the current book)
            var booksBySameAuthor = await _bookService.GetBooksBySameAuthor(id, book.AuthorId);

            // Prepare the ViewModel and pass it to the view
            var model = new BookWithSameAuthorViewModel
            {
                CurrentBook = book,
                BooksBySameAuthor = booksBySameAuthor
            };

            return View(model);
        }


        public async Task<IActionResult> BookList(int page = 1, int pageSize = 6, string search = null)
        {
            var data = await _bookService.GetBookPagination(page, pageSize, search);
            return View(data);
        }

        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _bookService.GetBookById(id);
            return PartialView(book);
        }


        [HttpGet("/downloadFile/{*filePath}")]
        public async Task<IActionResult> DownloadFile(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var memoryStream = await _fileService.DownloadFileAsync(filePath);
                memoryStream.Position = 0;

                return File(memoryStream, "application/octet-stream", fileName);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return NotFound("File not found in source project.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
