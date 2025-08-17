namespace MainChapar.Models
{
    public class UsedBook
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ContactNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsApproved { get; set; } // برای تأیید ادمین
       

        // ارتباط با کاربر
        public string UserId { get; set; } // FK به Identity User کسی که کتاب رو گذاشته
        public virtual User User { get; set; } // Navigation Property

        public string? ImageName { get; set; }

        public virtual ICollection<UsedBookImage> Images { get; set; } = new List<UsedBookImage>();
    }

    public class UsedBookImage
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }

        public int UsedBookId { get; set; }
        public virtual UsedBook UsedBook { get; set; }
    }
}
