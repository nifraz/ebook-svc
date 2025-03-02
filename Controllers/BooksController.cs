using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ebook_svc.Controllers
{
    [ApiController]
    [Route("books")]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;
        public BooksController(AppDbContext context) { _context = context; }

        // GET books/getBooks - public, list all books (approved ones for customers)
        [HttpGet("getBooks")]
        public IActionResult GetAllBooks()
        {
            // Return only books that are approved (so customers see only approved books)
            var books = _context.Books.Where(b => b.IsApproved)
                        .Include(b => b.Reviews)
                        .Select(b => new {
                            bookId = b.BookId,
                            bookName = b.BookName,
                            authorName = b.AuthorName,
                            price = b.Price,
                            quantity = b.Quantity,
                            description = b.Description,
                            imageURL = b.ImageURL,
                            isApproved = b.IsApproved,
                            isApprovalSent = b.IsApprovalSent,
                            rejectionCounts = b.RejectionCount,
                            averageRating = b.Reviews.Count > 0 ? b.Reviews.Average(r => r.Rating) : 0,
                            reviewCount = b.Reviews.Count,
                        }).ToList();
            return Ok(new { status = 200, message = "Success", data = books });
        }

        // GET books/getBookCount - public, returns total count of approved books
        [HttpGet("getBookCount")]
        public IActionResult GetBookCount()
        {
            int count = _context.Books.Count(b => b.IsApproved);
            return Ok(new { status = 200, message = "Success", data = count });
        }

        // GET books/getBooksByPriceDesc - public, books sorted by price descending
        [HttpGet("getBooksByPriceDesc")]
        public IActionResult GetBooksByPriceDesc()
        {
            var books = _context.Books.Where(b => b.IsApproved)
                            .Include(b => b.Reviews)
                            .Select(b => new {
                                bookId = b.BookId,
                                bookName = b.BookName,
                                authorName = b.AuthorName,
                                price = b.Price,
                                quantity = b.Quantity,
                                description = b.Description,
                                imageURL = b.ImageURL,
                                isApproved = b.IsApproved,
                                isApprovalSent = b.IsApprovalSent,
                                rejectionCounts = b.RejectionCount,
                                averageRating = b.Reviews.Count > 0 ? b.Reviews.Average(r => r.Rating) : 0,
                                reviewCount = b.Reviews.Count,
                            })
                            .OrderByDescending(b => b.price).ToList();
            return Ok(new { status = 200, message = "Success", data = books });
        }

        // GET books/getBooksByPriceAsc - public, books sorted by price ascending
        [HttpGet("getBooksByPriceAsc")]
        public IActionResult GetBooksByPriceAsc()
        {
            var books = _context.Books.Where(b => b.IsApproved)
                            .Include(b => b.Reviews)
                            .Select(b => new {
                                bookId = b.BookId,
                                bookName = b.BookName,
                                authorName = b.AuthorName,
                                price = b.Price,
                                quantity = b.Quantity,
                                description = b.Description,
                                imageURL = b.ImageURL,
                                isApproved = b.IsApproved,
                                isApprovalSent = b.IsApprovalSent,
                                rejectionCounts = b.RejectionCount,
                                averageRating = b.Reviews.Count > 0 ? b.Reviews.Average(r => r.Rating) : 0,
                                reviewCount = b.Reviews.Count,
                            })
                            .OrderBy(b => b.price).ToList();
            return Ok(new { status = 200, message = "Success", data = books });
        }

        // GET books/getBookByPage?pageNo=X - public, simple pagination
        [HttpGet("getBookByPage")]
        public IActionResult GetBookByPage(int pageNo = 1, int pageSize = 10)
        {
            if (pageNo < 1) pageNo = 1;
            var query = _context.Books.Where(b => b.IsApproved)
                            .Include(b => b.Reviews)
                            .Select(b => new {
                                bookId = b.BookId,
                                bookName = b.BookName,
                                authorName = b.AuthorName,
                                price = b.Price,
                                quantity = b.Quantity,
                                description = b.Description,
                                imageURL = b.ImageURL,
                                isApproved = b.IsApproved,
                                isApprovalSent = b.IsApprovalSent,
                                rejectionCounts = b.RejectionCount,
                                averageRating = b.Reviews.Count > 0 ? b.Reviews.Average(r => r.Rating) : 0,
                                reviewCount = b.Reviews.Count,
                            });
            int total = query.Count();
            var books = query.OrderBy(b => b.bookId)
                             .Skip((pageNo - 1) * pageSize)
                             .Take(pageSize).ToList();
            return Ok(new { status = 200, message = "Success", data = books, totalItems = total });
        }

        // GET books/bookStoreApplication/search?text=XYZ - public search by author
        [HttpGet("bookStoreApplication/search")]
        public IActionResult SearchByText([FromQuery] string text)
        {
            var books = _context.Books.Where(b => b.IsApproved && (b.AuthorName.Contains(text) || b.BookName.Contains(text)))
                            .Include(b => b.Reviews)
                            .Select(b => new {
                                bookId = b.BookId,
                                bookName = b.BookName,
                                authorName = b.AuthorName,
                                price = b.Price,
                                quantity = b.Quantity,
                                description = b.Description,
                                imageURL = b.ImageURL,
                                isApproved = b.IsApproved,
                                isApprovalSent = b.IsApprovalSent,
                                rejectionCounts = b.RejectionCount,
                                averageRating = b.Reviews.Count > 0 ? b.Reviews.Average(r => r.Rating) : 0,
                                reviewCount = b.Reviews.Count,
                            })
                            .ToList();
            return Ok(new { status = 200, message = "Success", data = books });
        }
    }

}
