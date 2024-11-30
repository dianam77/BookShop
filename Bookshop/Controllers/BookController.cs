using Bookshop.Models;
using Core.BookService;
using Core.ServiceFile;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            var book = await _bookService.GetBookById(id);
            if (book == null)
            {
                return NotFound("Book not found.");
            }

            var booksBySameAuthor = await _bookService.GetBooksBySameAuthor(id, book.AuthorId);
            var comments = await _bookService.GetCommentsByBookId(id);

            var model = new BookWithSameAuthorViewModel
            {
                CurrentBook = book,
                BooksBySameAuthor = booksBySameAuthor,
                Comments = comments,
                CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) // Set current user ID
            };

            return View(model);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int bookId, string text, int? replyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity.Name;

            var comment = new Comment
            {
                BookId = bookId,
                Text = text,
                Created = DateTime.Now,
                UserId = userId,
                UserName = userName,
                ReplyId = replyId
            };

            await _bookService.AddComment(comment);
            return RedirectToAction("Index", new { id = bookId });
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id, int bookId)
        {
            var comment = await _bookService.GetCommentById(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (comment.UserId == userId)
            {
                await _bookService.DeleteComment(id);
            }
            return RedirectToAction("Index", new { id = bookId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditComment(int id, int bookId, string newText)
        {
            var comment = await _bookService.GetCommentById(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (comment.UserId == userId)
            {
                comment.Text = newText;
                await _bookService.UpdateComment(comment);
            }
            return RedirectToAction("Index", new { id = bookId });
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
