using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ebook_svc.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int BookId { get; set; }
        [ValidateNever]
        public Book Book { get; set; }

        public int UserId { get; set; }
        [ValidateNever]
        public User User { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        public decimal TotalPrice => Book != null ? Book.Price * Quantity : 0;
    }

}
