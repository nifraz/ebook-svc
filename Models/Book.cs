using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ebook_svc.Models
{
    public class Book
    {
        public int BookId { get; set; }

        [Required/*, MaxLength(150)*/]
        public string BookName { get; set; }

        [Required/*, MaxLength(100)*/]
        public string AuthorName { get; set; }

        [Required]
        //[Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        public decimal Price { get; set; }

        [Required]
        //[Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }

        [Required/*, MaxLength(1000)*/]
        public string Description { get; set; }

        public string? ImageURL { get; set; }

        public bool IsApproved { get; set; } = false;       // approved by admin?
        public bool IsApprovalSent { get; set; } = false;   // vendor requested approval?
        public int RejectionCount { get; set; } = 0;        // times admin rejected

        // Foreign key to seller (vendor user)
        public int SellerId { get; set; }
        [ValidateNever]
        public User Seller { get; set; }

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; } = [];
        public ICollection<WishList> WishLists { get; set; } = [];
        public ICollection<CartItem> CartItems { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
    }

}
