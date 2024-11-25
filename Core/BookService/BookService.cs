using Core.ServiceFile;
using DataAccess.Models;
using DataAccess.Repositories.BookRepo;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Core.BookService
{
    public class BookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IFileService _fileService;
        public BookService(IBookRepository bookRepository, IFileService fileService)
        {
            _bookRepository = bookRepository;
            _fileService = fileService;
        }


        public async Task<IEnumerable<Book>> GetBooks()
        {
            return await _bookRepository.GetAll().ToListAsync();
        }
        public async Task<IEnumerable<Book>> GetBooksBySameAuthor(int bookId, int authorId)
        {
            // Get books with the same author but excluding the current book
            var booksBySameAuthor = await _bookRepository
                .GetAll()
                .Where(b => b.AuthorId == authorId && b.Id != bookId)  // Filter by AuthorId and exclude the current book
                .Include(b => b.Author)  // Include the Author information
                .ToListAsync();  // Execute the query and return the list

            return booksBySameAuthor;
        }

        public async Task<IEnumerable<Book>> GetBooksWithAuthors(Expression<Func<Book, bool>> where = null)
        {
            return await _bookRepository.GetAll().Include(a => a.Author).ToListAsync();
        }
        public async Task<Book> GetBookById(int id)
        {
            return await _bookRepository.GetById(id);
        }

        
        public async Task CreateBook(BookDto bookdto)
        {
            var book = new Book
            {
                AuthorId = bookdto.AuthorId,
                Title = bookdto.Title,
                Description = bookdto.Description,
                Price = bookdto.Price,
                IsAvail = bookdto.IsAvail,
                ShowHomePage = bookdto.ShowHomePage,
                Created = DateTime.Now,
            };

            try
            {
                // Check if there is an uploaded image
                if (bookdto.Img != null)
                {
                    // Use the Save method of _fileService to save the image and get the file name
                    var result = await _fileService.UploadStaticFile(bookdto.Img, "BookImages");
                    book.Img = result.FileAddress;
                }

                // Save the book to the repository
                await _bookRepository.Add(book);

               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while uploading the file or saving the book: {ex.Message}");
                // Optionally handle the error, e.g., log it or throw a custom exception
            }
        }

        public async Task UpdateBook(BookDto bookdto)
        {
            var book = await GetBookById(bookdto.Id);
            if (book == null)
            {
                throw new ArgumentException("Book not found");
            }

            // Update book properties
            book.Title = bookdto.Title;
            book.Description = bookdto.Description;
            book.Price = bookdto.Price;
            book.AuthorId = bookdto.AuthorId;
            book.IsAvail = bookdto.IsAvail;
            book.ShowHomePage = bookdto.ShowHomePage;
            book.Created = DateTime.Now;

            // Check if a new image was uploaded
            if (bookdto.Img != null)
            {
                // Optionally delete the old image if it exists
                if (!string.IsNullOrEmpty(book.Img))
                {
                    _fileService.DeleteFile(book.Img
                        );
                }

                
                var result= await _fileService.UploadStaticFile(bookdto.Img, "BookImages");
                book.Img = result.FileAddress;
            }

            // Update the book in the repository
            await _bookRepository.Update(book);
        }


        public async Task DeleteBook(Book book)
        {
            await _bookRepository.Delete(book);
        }
        public async Task<IEnumerable<Book>> GetBooksByAuthor(int authorId, int excludeBookId)
        {
            return await _bookRepository.GetAll()
                .Where(b => b.AuthorId == authorId && b.Id != excludeBookId)
                .ToListAsync();
        }


        public async Task<BookDto> GetBookDtoById(int id)
        {
            var book = await _bookRepository.GetById(id);

            if (book == null)
            {
                return null;  // Handle the case where the book is not found
            }

            var bookDto = new BookDto()
            {
                AuthorId = book.AuthorId,
                ImgName = book.Img,
                Description = book.Description,
                Id = id,
                Price = book.Price,
                Title = book.Title,
                IsAvail = book.IsAvail,
                ShowHomePage = book.ShowHomePage
            };

            return bookDto;
        }

        public async Task<PagedBookDto> GetBookPagination(int page, int pageSize, string? search)
        {
            var books = _bookRepository.GetAll();

            // Filter by search term if provided
            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(a => a.Title.Contains(search) || a.Description.Contains(search));
            }

            // Count total books for pagination
            int totalCount = await books.CountAsync();  // CountAsync for async operation
            int totalPage = (int)Math.Ceiling((double)totalCount / pageSize);

            // Apply pagination
            books = books.Skip((page - 1) * pageSize).Take(pageSize);

            // Include the Author entity, not the AuthorId
            books = books.Include(a => a.Author);

            // Select required fields for the DTO
            var bookDto = await books.Select(s => new BookDto()
            {
                Title = s.Title,
                Price = s.Price,
                AuthorId = s.AuthorId,  // AuthorId is just a scalar property, so you can include it directly here
                Description = s.Description,
                Id = s.Id,
                AuthorName = s.Author.Name,  // Access the related Author's Name
                ImgName = s.Img
            }).ToListAsync();

            // Prepare the final paginated result
            var result = new PagedBookDto()
            {
                Items = bookDto,
                page = page,
                totalPage = totalPage,
            };

            return result;
        }


    }


}
