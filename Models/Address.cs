using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ebook_svc.Models
{
    public class Address
    {
        public int AddressId { get; set; }
        public int UserId { get; set; }
        [ValidateNever]
        public User User { get; set; }

        [Required] public string Name { get; set; }
        [Required] public string PhoneNumber { get; set; }
        [Required] public string Pincode { get; set; }
        [Required] public string Locality { get; set; }
        [Required] public string AddressLine { get; set; }
        [Required] public string City { get; set; }
        public string Landmark { get; set; }
        [Required] public string AddressType { get; set; }  // e.g., "Home", "Work"
    }

}
