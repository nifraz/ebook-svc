using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ebook_svc.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int BookId { get; set; }
        [ValidateNever]
        public Book Book { get; set; }

        public int UserId { get; set; }
        [ValidateNever]
        public User User { get; set; }

        [Required/*, MaxLength(1000)*/]
        public string Content { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
