using ebook_svc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ebook_svc.Controllers
{
    [ApiController]
    [Route("admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context) { _context = context; }

        // GET admin/getSellersForVerification
        [HttpGet("getSellersForVerification")]
        public IActionResult GetSellersForVerification()
        {
            // Get all vendors who have at least one book awaiting approval
            var sellers = _context.Users
                           .Where(u => u.Role == Role.Vendor &&
                                       u.Books.Any(b => b.IsApprovalSent && !b.IsApproved))
                           .Select(u => new { id = u.Id, name = u.Name, userName = u.UserName })
                           .ToList();
            return Ok(new { status = 200, message = "Success", data = sellers });
        }

        // GET admin/getBooksForVerification/{sellerId}
        [HttpGet("getBooksForVerification/{sellerId}")]
        public IActionResult GetBooksForVerification(int sellerId)
        {
            // All books of this seller that are submitted for approval and not yet approved
            var books = _context.Books.Where(b => b.SellerId == sellerId && b.IsApprovalSent && !b.IsApproved)
                        .Select(b => new {
                            bookId = b.BookId,
                            bookName = b.BookName,
                            authorName = b.AuthorName,
                            price = b.Price,
                            quantity = b.Quantity,
                            description = b.Description,
                            rejectionCounts = b.RejectionCount
                        }).ToList();
            return Ok(new { status = 200, message = "Success", data = books });
        }

        // PUT admin/bookVerification/{bookId}/{sellerId}/{verification}
        [HttpPut("bookVerification/{bookId}/{sellerId}/{verification}")]
        public IActionResult VerifyBook(int bookId, int sellerId, string verification,
                                        [FromBody] string description = "")
        {
            bool approve = verification.ToLower() == "true";
            var book = _context.Books.FirstOrDefault(b => b.BookId == bookId && b.SellerId == sellerId);
            if (book == null) return NotFound(new { status = 404, message = "Book not found" });
            if (approve)
            {
                book.IsApproved = true;
                book.IsApprovalSent = false;
                // Optionally, reset rejection count or leave it as history
            }
            else
            {
                book.IsApproved = false;
                book.IsApprovalSent = false;
                book.RejectionCount += 1;
                // Could log the description (reason) somewhere, e.g., Book.LastRejectionReason = description
            }
            _context.SaveChanges();
            string msg = approve ? "Book approved" : "Book rejected";
            return Ok(new { status = 200, message = msg });
        }
    }

}
