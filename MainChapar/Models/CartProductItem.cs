using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class CartProductItem
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsFinalized { get; set; } = false; // بعد از ثبت سفارش میشه true

        // محاسبه قیمت نهایی برای این آیتم
        public decimal TotalPrice => (Product?.Price ?? 0 - Product?.Discount ?? 0) * Quantity;
    }
}
