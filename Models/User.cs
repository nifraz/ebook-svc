using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ebook_svc.Models
{
    public enum Role { Admin = 1, Vendor = 2, Customer = 3 }

    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [EmailAddress]  // ensure valid email format
        [RegularExpression(@"[A-Za-z0-9][A-Za-z0-9_.]*@[A-Za-z0-9]+(\.[A-Za-z]+)+",
            ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required, MinLength(3), MaxLength(50)]
        [RegularExpression(@"^[A-Za-z]+(\s[A-Za-z]+)*$",
            ErrorMessage = "Name can only contain letters and spaces")]
        public string Name { get; set; }

        [Required, MinLength(4), MaxLength(16)]
        [RegularExpression(@"^[A-Za-z0-9]+$",
            ErrorMessage = "Username can only contain letters and numbers")]
        public string UserName { get; set; }

        [Required, MinLength(8), MaxLength(16)]
        [RegularExpression(@"(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,16}$",
            ErrorMessage = "Password must be 8-16 chars, with at least 1 digit, 1 uppercase, 1 lowercase")]
        [JsonIgnore]  // do not expose hashed password in API responses
        public string Password { get; set; }

        [Required, MaxLength(15)]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone number must be 10 digits")]
        public string MobileNumber { get; set; }

        public Role Role { get; set; }  // Admin=1, Vendor=2, Customer=3

        public bool IsVerified { get; set; } = false;  // email (or admin) verification status

        public string? ImageData { get; set; }

        // Navigation properties
        public ICollection<Book> Books { get; set; } = [];
        public ICollection<Order> Orders { get; set; } = [];
        public ICollection<WishList> WishListItems { get; set; } = [];
        public ICollection<CartItem> CartItems { get; set; } = [];
    }

}
