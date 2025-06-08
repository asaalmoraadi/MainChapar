namespace MainChapar.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsConfirmed { get; set; } = false; // تأیید دستی یا اتومات
        public bool IsCollected { get; set; } = false; // آیا کاربر محصولو دریافت کرده
        public string PickupCode { get; set; } // اینو تبدیل به QR می‌کنیم

        // پراپرتی محاسبه‌ای برای قیمت کل
        public decimal TotalPrice
        {
            get
            {
                return OrderDetails?.Sum(od => od.Quantity * od.UnitPrice) ?? 0;
            }
        }

        //nav to user
        public string UserId { get; set; }
        public User User { get; set; }
        // nav to OrderDetails
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
