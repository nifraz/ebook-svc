using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ebook_svc.Models
{
    public class WishList
    {
        public int WishListId { get; set; }
        public int UserId { get; set; }
        [ValidateNever]
        public User User { get; set; }

        public int BookId { get; set; }
        [ValidateNever]
        public Book Book { get; set; }
    }

}
