using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ebook_svc.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        [ValidateNever]
        public Order Order { get; set; }

        // Book details at time of order:
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string AuthorName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal TotalPrice => Price * Quantity;
    }
}
