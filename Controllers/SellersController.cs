using ebook_svc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ebook_svc.Controllers
{
    [ApiController]
    [Route("sellers")]
    [Authorize(Roles = "Vendor")]
    public class SellersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public SellersController(AppDbContext context) { _context = context; }

        // POST sellers/addBook
        [HttpPost("addBook")]
        public IActionResult AddBook([FromBody] Book bookDto)
        {
            int vendorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            // Create new book with vendor as seller
            var book = new Book
            {
                BookName = bookDto.BookName,
                AuthorName = bookDto.AuthorName,
                Price = bookDto.Price,
                Quantity = bookDto.Quantity,
                Description = bookDto.Description,
                SellerId = vendorId,
                IsApproved = false,
                IsApprovalSent = false,
                RejectionCount = 0,
                ImageData = bookDto.ImageData,
            };
            _context.Books.Add(book);
            _context.SaveChanges();
            // Notify the count of books (optional)
            int count = _context.Books.Count(b => b.SellerId == vendorId);
            return StatusCode(201, new { status = 201, message = "Book added successfully", data = new { booksCount = count, bookId = book.BookId } });
        }

        // PUT sellers/updateBook/{bookId}
        [HttpPut("updateBook/{bookId}")]
        public IActionResult UpdateBook(int bookId, [FromBody] Book updateDto)
        {
            int vendorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var book = _context.Books.FirstOrDefault(b => b.BookId == bookId && b.SellerId == vendorId);
            if (book == null) return NotFound(new { status = 404, message = "Book not found" });
            // Update fields
            book.BookName = updateDto.BookName;
            book.AuthorName = updateDto.AuthorName;
            book.Price = updateDto.Price;
            book.Quantity = updateDto.Quantity;
            book.Description = updateDto.Description;
            // If a book is updated after being approved, mark it needing re-approval (optional logic)
            if (book.IsApproved)
            {
                book.IsApproved = false;
                book.IsApprovalSent = false;
                book.RejectionCount = 0;
            }
            _context.SaveChanges();
            return Ok(new { status = 200, message = "Book updated successfully" });
        }

        // DELETE sellers/removeBook/{bookId}
        [HttpDelete("removeBook/{bookId}")]
        public IActionResult RemoveBook(int bookId)
        {
            int vendorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var book = _context.Books.Include(b => b.OrderItems)
                         .FirstOrDefault(b => b.BookId == bookId && b.SellerId == vendorId);
            if (book == null) return NotFound(new { status = 404, message = "Book not found" });
            // If the book has any orders in the past, you might choose not to delete. Here, check and warn:
            if (book.OrderItems != null && book.OrderItems.Any())
            {
                // We won't delete books that have been ordered; instead mark as unavailable
                book.Quantity = 0;
                book.IsApproved = false;
                _context.SaveChanges();
                return Ok(new { status = 200, message = "Book marked as removed (had past orders)" });
            }
            // If no past orders, safe to remove entirely
            _context.Books.Remove(book);
            _context.SaveChanges();
            return Ok(new { status = 200, message = "Book removed successfully" });
        }

        // GET sellers/displayBooks?pageNo=X
        [HttpGet("displayBooks")]
        public IActionResult DisplayBooks(int pageNo = 1, int pageSize = 5)
        {
            int vendorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var query = _context.Books.Where(b => b.SellerId == vendorId)
                                      .OrderBy(b => b.BookId);
            int total = query.Count();
            var booksPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new { status = 200, message = "Success", data = booksPage, totalBooks = total });
        }

        // PUT sellers/approvalSent/{bookId}
        [HttpPut("approvalSent/{bookId}")]
        public IActionResult SendForApproval(int bookId)
        {
            int vendorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var book = _context.Books.FirstOrDefault(b => b.BookId == bookId && b.SellerId == vendorId);
            if (book == null) return NotFound(new { status = 404, message = "Book not found" });
            // Mark as pending admin approval
            book.IsApprovalSent = true;
            _context.SaveChanges();
            return Ok(new { status = 200, message = "Book submitted for approval" });
        }

        // GET sellers/search/{query}
        [HttpGet("search/{query}")]
        public IActionResult SearchMyBooks(string query)
        {
            int vendorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var results = _context.Books.Where(b => b.SellerId == vendorId && b.BookName.Contains(query))
                                        .ToList();
            return Ok(new { status = 200, message = "Success", data = results });
        }

        // GET sellers/booksCount
        [HttpGet("booksCount")]
        public IActionResult GetBooksCount()
        {
            int vendorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            int count = _context.Books.Count(b => b.SellerId == vendorId);
            return Ok(new { status = 200, message = "Success", data = count });
        }
    }

}
