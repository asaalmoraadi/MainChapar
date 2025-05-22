using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }       // آیدی کاربر
        public User User { get; set; }
       
        [Required]
        [MaxLength(1000)]
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
