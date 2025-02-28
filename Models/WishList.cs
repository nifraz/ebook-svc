namespace ebook_svc.Models
{
    public class WishList
    {
        public int WishListId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; }
    }

}
