using ebook_svc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ebook_svc.Controllers
{
    [ApiController]
    [Route("carts")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CartController(AppDbContext context) { _context = context; }

        // POST carts/addToCart/{bookId}
        [Authorize(Roles = "Customer")]
        [HttpPost("addToCart/{bookId}")]
        public IActionResult AddToCart(int bookId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var existingItem = _context.CartItems.FirstOrDefault(ci => ci.UserId == userId && ci.BookId == bookId);
            if (existingItem != null)
            {
                // If already in cart, we won't add again. Return 208 so UI knows it's already there.
                return Ok(new { status = 208, message = "Book already in cart" });
            }
            // Add new cart item with quantity 1
            var book = _context.Books.Find(bookId);
            if (book == null || !book.IsApproved || book.Quantity <= 0)
            {
                return StatusCode(417, new { status = 417, message = "Book not available" }); // Expectation Failed if out-of-stock
            }
            var cartItem = new CartItem { UserId = userId, BookId = bookId, Quantity = 1 };
            _context.CartItems.Add(cartItem);
            _context.SaveChanges();
            // Return new cart size 
            int cartCount = _context.CartItems.Count(ci => ci.UserId == userId);
            return Ok(new { status = 200, message = "Book added to cart", data = new { totalBooksInCart = cartCount } });
        }

        // GET carts/displayItems
        [Authorize(Roles = "Customer")]
        [HttpGet("displayItems")]
        public IActionResult DisplayCartItems()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var items = _context.CartItems.Where(ci => ci.UserId == userId)
                        .Include(ci => ci.Book).ToList();
            // Build cart details
            var cartBooks = items.Select(ci => new {
                cartBookId = ci.CartItemId,
                bookQuantity = ci.Quantity,
                totalBookPrice = ci.Book.Price * ci.Quantity,
                book = new
                {
                    bookId = ci.BookId,
                    bookName = ci.Book.BookName,
                    authorName = ci.Book.AuthorName,
                    price = ci.Book.Price,
                    quantity = ci.Book.Quantity,
                    description = ci.Book.Description,
                    imageURL = ci.Book.ImageData,
                    isApproved = ci.Book.IsApproved
                }
            }).ToList();
            var responseData = new
            {
                cartId = 0,  // not really used
                totalBooksInCart = cartBooks.Count,
                cartBooks = cartBooks
            };
            return Ok(new { status = 200, message = "Success", data = responseData });
        }

        // DELETE carts/removeFromCart/{cartBookId}
        [Authorize(Roles = "Customer")]
        [HttpDelete("removeFromCart/{cartItemId}")]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var item = _context.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.UserId == userId);
            if (item == null) return NotFound(new { status = 404, message = "Cart item not found" });
            _context.CartItems.Remove(item);
            _context.SaveChanges();
            int cartCount = _context.CartItems.Count(ci => ci.UserId == userId);
            return Ok(new { status = 200, message = "Removed from cart", data = new { totalBooksInCart = cartCount } });
        }

        // PUT carts/addQuantity/{cartItemId}
        [Authorize(Roles = "Customer")]
        [HttpPut("addQuantity/{cartItemId}")]
        public IActionResult IncreaseQuantity(int cartItemId)
        {
            return ChangeQuantity(cartItemId, +1);
        }

        // PUT carts/removeQuantity/{cartItemId}
        [Authorize(Roles = "Customer")]
        [HttpPut("removeQuantity/{cartItemId}")]
        public IActionResult DecreaseQuantity(int cartItemId)
        {
            return ChangeQuantity(cartItemId, -1);
        }

        // Helper for adjusting quantity
        private IActionResult ChangeQuantity(int cartItemId, int delta)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var item = _context.CartItems.Include(ci => ci.Book)
                         .FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.UserId == userId);
            if (item == null) return NotFound(new { status = 404, message = "Cart item not found" });
            if (delta > 0)
            {
                // increase quantity
                if (item.Book.Quantity <= item.Quantity) // no more stock
                    return StatusCode(417, new { status = 417, message = "No more stock available for this book" });
                item.Quantity += delta;
            }
            else if (delta < 0)
            {
                // decrease quantity
                item.Quantity += delta;
                if (item.Quantity <= 0)
                {
                    // remove item if quantity goes to 0
                    _context.CartItems.Remove(item);
                }
            }
            _context.SaveChanges();
            return Ok(new { status = 200, message = "Quantity updated" });
        }

        // PUT carts/updateQuantity/{cartItemId}/{quantity}
        [Authorize(Roles = "Customer")]
        [HttpPut("updateQuantity/{cartItemId}/{quantity}")]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var item = _context.CartItems.Include(ci => ci.Book)
                         .FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.UserId == userId);
            if (item == null) return NotFound(new { status = 404, message = "Cart item not found" });
            if (quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                if (quantity > item.Book.Quantity)
                {
                    return StatusCode(417, new { status = 417, message = "Requested quantity not available" });
                }
                item.Quantity = quantity;
            }
            _context.SaveChanges();
            return Ok(new { status = 200, message = "Quantity updated" });
        }

        // GET carts/cartSize
        [Authorize]
        [HttpGet("cartSize")]
        public IActionResult CartSize()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            int count = _context.CartItems.Count(ci => ci.UserId == userId);
            return Ok(new { status = 200, message = "Success", data = count });
        }

        // POST carts/placeOrder  (Add guest cart items to logged-in user's cart)
        [Authorize(Roles = "Customer")]
        [HttpPost("placeOrder")]
        public IActionResult PlaceOrder([FromBody] CartModule localCart)
        {
            // The client may send the local cart items after login to sync with server cart
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (localCart?.cartBooks != null)
            {
                foreach (var item in localCart.cartBooks)
                {
                    int bookId = item.book.bookId;
                    var existing = _context.CartItems.FirstOrDefault(ci => ci.UserId == userId && ci.BookId == bookId);
                    if (existing != null) continue; // skip items already in cart
                    var book = _context.Books.Find(bookId);
                    if (book != null && book.IsApproved && book.Quantity > 0)
                    {
                        _context.CartItems.Add(new CartItem { UserId = userId, BookId = bookId, Quantity = item.bookQuantity });
                    }
                }
                _context.SaveChanges();
            }
            int count = _context.CartItems.Count(ci => ci.UserId == userId);
            return Ok(new { status = 200, message = "Cart synchronized", data = new { totalBooksInCart = count } });
        }
    }

    // For binding the local cart JSON (matching Angular CartModule structure)
    public class CartModule
    {
        public int cartId { get; set; }
        public int totalBooksInCart { get; set; }
        public List<CartBookDto> cartBooks { get; set; }
    }
    public class CartBookDto
    {
        public int cartBookId { get; set; }
        public int bookQuantity { get; set; }
        public BookDto book { get; set; }
    }
    public class BookDto
    {
        public int bookId { get; set; }
        public string bookName { get; set; }
        public string authorName { get; set; }
        public decimal price { get; set; }
        public int quantity { get; set; }
        public string description { get; set; }
        public string imageURL { get; set; }
    }

}
