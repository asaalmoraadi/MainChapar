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
     
        public string PickupCode { get; set; } // اینو تبدیل به QR می‌کنیم


        //public string Status { get; set; }
        

        //nav to user
        public string UserId { get; set; }
        public User User { get; set; }

        public ICollection<PickupProductItem> ProductItems { get; set; } = new List<PickupProductItem>();
        public ICollection<PickupPrintItem> PickupPrintItems { get; set; } = new List<PickupPrintItem>();


    }
}
