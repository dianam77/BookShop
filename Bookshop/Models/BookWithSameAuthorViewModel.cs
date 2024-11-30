using DataAccess.Models;


namespace Bookshop.Models
{
    public class BookWithSameAuthorViewModel
    {
        public Book CurrentBook { get; set; }
        public IEnumerable<Book> BooksBySameAuthor { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public Comment NewComment { get; set; }
        public string CurrentUserId { get; set; } // Add this property
    }

}
