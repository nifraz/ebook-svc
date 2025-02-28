using ebook_svc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ebook_svc.Controllers
{
    [ApiController]
    [Route("review")]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ReviewController(AppDbContext context) { _context = context; }

        // POST review/{bookId}/{orderId}
        //[Authorize(Roles = "Customer")]
        [HttpPost("{bookId}/{orderId}")]
        public IActionResult AddReview(int bookId, int orderId, [FromBody] ReviewDto reviewDto)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            // Verify that this user has an order with this book
            bool purchased = _context.OrderItems.Any(oi => oi.OrderId == orderId && oi.BookId == bookId
                                                           && oi.Order.UserId == userId);
            if (!purchased)
            {
                return StatusCode(403, new { status = 403, message = "You can only review books you have purchased" });
            }

            // Check if already reviewed (one review per user-book maybe)
            bool alreadyReviewed = _context.Reviews.Any(r => r.BookId == bookId && r.UserId == userId);
            if (alreadyReviewed)
            {
                return BadRequest(new { status = 400, message = "You have already reviewed this book" });
            }
            // Add new review
            var review = new Review
            {
                BookId = bookId,
                UserId = userId,
                Content = reviewDto.Review,
                Rating = reviewDto.Rating
            };
            _context.Reviews.Add(review);
            _context.SaveChanges();
            return Ok(new { status = 200, message = "Review added", data = new { review = review.Content, rating = review.Rating } });
        }

        // GET review/{bookId}
        //[Authorize]  // any logged-in user can fetch reviews (could also allow anonymous if desired)
        [HttpGet("{bookId}")]
        public IActionResult GetReviews(int bookId)
        {
            var reviews = _context.Reviews.Include(r => r.User)
                            .Where(r => r.BookId == bookId)
                            .Select(r => new {
                                user = r.User.Name,
                                rating = r.Rating,
                                review = r.Content,
                                date = r.CreatedAt
                            }).ToList();
            return Ok(new { status = 200, message = "Success", data = reviews });
        }

        // POST reviewApp
        //[Authorize(Roles = "Customer")]
        [HttpPost("~/reviewApp")]
        public IActionResult AddQuickReview([FromBody] ReviewDto reviewDto)
        {
            // If the UI uses reviewApp without specifying book (e.g., on success page), 
            // we might interpret that as reviewing the last order's first item:
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var lastOrderItem = _context.OrderItems
                                .Where(oi => oi.Order.UserId == userId)
                                .OrderByDescending(oi => oi.Order.OrderDate)
                                .FirstOrDefault();
            if (lastOrderItem == null)
                return BadRequest(new { status = 400, message = "No recent purchase found to review" });
            // Reuse AddReview logic
            var dto = new ReviewDto { Review = reviewDto.Review, Rating = reviewDto.Rating };
            return AddReview(lastOrderItem.BookId, lastOrderItem.OrderId, new ReviewDto { Review = reviewDto.Review, Rating = reviewDto.Rating });
        }
    }

    public class ReviewDto
    {
        public string Review { get; set; }
        public int Rating { get; set; }
    }

}
