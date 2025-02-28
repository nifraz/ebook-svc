using ebook_svc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ebook_svc.Controllers
{
    [ApiController]
    [Route("wishlists")]
    public class WishlistsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public WishlistsController(AppDbContext context) { _context = context; }

        // POST wishlists/addToWishlist/{bookId}
        [Authorize(Roles = "Customer")]
        [HttpPost("addToWishlist/{bookId}")]
        public IActionResult AddToWishlist(int bookId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            // Check if already in wishlist
            bool exists = _context.WishLists.Any(w => w.UserId == userId && w.BookId == bookId);
            if (exists)
            {
                return Ok(new { status = 208, message = "Book already in wishlist" });
            }
            // Add new entry
            var wish = new WishList { UserId = userId, BookId = bookId };
            _context.WishLists.Add(wish);
            _context.SaveChanges();
            // Return updated wishlist count as data
            int count = _context.WishLists.Count(w => w.UserId == userId);
            return Ok(new { status = 200, message = "Added to wishlist", data = new { totalItems = count } });
        }

        // GET wishlists/displayItems
        [Authorize]
        [HttpGet("displayItems")]
        public IActionResult ViewWishlist()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var wishItems = _context.WishLists
                             .Where(w => w.UserId == userId)
                             .Include(w => w.Book)
                             .ToList();
            // Prepare data: list of books in wishlist
            var books = wishItems.Select(w => new {
                bookId = w.BookId,
                bookName = w.Book.BookName,
                authorName = w.Book.AuthorName,
                price = w.Book.Price,
                quantity = w.Book.Quantity,
                description = w.Book.Description,
                imageURL = w.Book.ImageData,
                isApproved = w.Book.IsApproved
            }).ToList();
            return Ok(new { status = 200, message = "Success", data = books });
        }

        // DELETE wishlists/removeFromWishlist/{bookId}
        [Authorize]
        [HttpDelete("removeFromWishlist/{bookId}")]
        public IActionResult RemoveFromWishlist(int bookId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var wish = _context.WishLists.FirstOrDefault(w => w.UserId == userId && w.BookId == bookId);
            if (wish == null)
            {
                return NotFound(new { status = 404, message = "Item not found in wishlist" });
            }
            _context.WishLists.Remove(wish);
            _context.SaveChanges();
            // Return remaining count
            int count = _context.WishLists.Count(w => w.UserId == userId);
            return Ok(new { status = 200, message = "Removed from wishlist", data = new { totalItems = count } });
        }
    }

}
