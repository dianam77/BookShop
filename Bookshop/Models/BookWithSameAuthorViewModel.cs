using DataAccess.Models;

namespace Bookshop.Models
{
    public class BookWithSameAuthorViewModel
    {
        public Book CurrentBook { get; set; }
        public IEnumerable<Book> BooksBySameAuthor { get; set; }
    }

}
