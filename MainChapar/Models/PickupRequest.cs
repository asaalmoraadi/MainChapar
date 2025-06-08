using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class PickupRequest
    {
        [Key]
        public int Id { get; set; }
        public string QrCodeToken { get; set; } // برای تولید QR یکتا
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDelivered { get; set; } = false;

        public string Status { get; set; }
        // وضعیت‌ها:
        // "در انتظار تأیید"
        // "تأیید شده"
        // "در حال آماده‌سازی"
        // "آماده برای تحویل"
        // "تحویل داده شده"
        // "رد شده"

        //nav to user
        public string UserId { get; set; }
        public User User { get; set; }

        public ICollection<PickupProductItem> ProductItems { get; set; } = new List<PickupProductItem>();
        public ICollection<PickupPrintItem> PickupPrintItems { get; set; } = new List<PickupPrintItem>();


    }
}
