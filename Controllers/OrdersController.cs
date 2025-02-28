using ebook_svc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ebook_svc.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrdersController(AppDbContext context) { _context = context; }

        // POST orders/checkout
        //[Authorize(Roles = "Customer")]
        [HttpPost("checkOut")]
        public IActionResult Checkout()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            // Retrieve user cart items
            var cartItems = _context.CartItems.Include(ci => ci.Book)
                                .Where(ci => ci.UserId == userId).ToList();
            if (!cartItems.Any())
            {
                return BadRequest(new { status = 400, message = "Cart is empty" });
            }
            // Validate stock for each item
            foreach (var ci in cartItems)
            {
                if (ci.Book.Quantity < ci.Quantity)
                {
                    return StatusCode(417, new { status = 417, message = $"Not enough stock for {ci.Book.BookName}" });
                }
            }
            // Create order
            var order = new Order { UserId = userId, OrderDate = DateTime.UtcNow };
            // Optionally attach an address if exists (take default type "Home")
            var address = _context.Addresses.FirstOrDefault(a => a.UserId == userId && a.AddressType.ToLower() == "home");
            if (address != null) order.AddressId = address.AddressId;
            _context.Orders.Add(order);
            _context.SaveChanges();
            // Create order items
            foreach (var ci in cartItems)
            {
                order.OrderItems = order.OrderItems ?? new List<OrderItem>();
                order.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    BookId = ci.BookId,
                    BookName = ci.Book.BookName,
                    AuthorName = ci.Book.AuthorName,
                    Price = ci.Book.Price,
                    Quantity = ci.Quantity
                });
                // Deduct stock
                ci.Book.Quantity -= ci.Quantity;
            }
            // Clear the cart
            _context.CartItems.RemoveRange(cartItems);
            _context.SaveChanges();
            return Ok(new { status = 200, message = "Order placed successfully", data = new { orderId = order.OrderId } });
        }

        // POST orders/addMyOrder (alias for checkout)
        //[Authorize(Roles = "Customer")]
        [HttpPost("addMyOrder")]
        public IActionResult AddMyOrder()
        {
            // For backward compatibility with any Angular calls to addMyOrder, perform checkout
            return Checkout();
        }

        // GET orders/myorders
        //[Authorize(Roles = "Customer")]
        [HttpGet("myorders")]
        public IActionResult GetMyOrders()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var orders = _context.Orders.Include(o => o.OrderItems)
                            .Where(o => o.UserId == userId)
                            .OrderByDescending(o => o.OrderDate)
                            .ToList();
            // Shape data: list orders with items
            var data = orders.Select(o => new {
                orderId = o.OrderId,
                orderDate = o.OrderDate,
                items = o.OrderItems.Select(oi => new {
                    bookId = oi.BookId,
                    bookName = oi.BookName,
                    quantity = oi.Quantity,
                    price = oi.Price,
                    totalPrice = oi.Price * oi.Quantity
                }).ToList()
            });
            return Ok(new { status = 200, message = "Success", data = data });
        }
    }

}
