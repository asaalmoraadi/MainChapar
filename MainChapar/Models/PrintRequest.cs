using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class PrintRequest
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } // اتصال به Identity
        public User User { get; set; }

        public string ServiceType { get; set; } // Color, BlackWhite, Plan,laminate

        public string Status { get; set; } = "Submitted"; // Submitted, Processing, Completed, Rejected
        // در صورتی مه ثبت نهایی شد true میشود
        //مبحث flag
        public bool IsFinalized { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public decimal? TotalPrice { get; set; }
        public ColorPrintDetail ColorPrintDetail { get; set; }
        public BlackWhitePrintDetail BlackWhitePrintDetail { get; set; }
        public PlanPrintDetail PlanPrintDetail { get; set; }
        public LaminateDetail LaminateDetail { get; set; }

        public ICollection<PrintFile> Files { get; set; }
        public ICollection<PickupPrintItem> PickupPrintItems { get; set; } = new List<PickupPrintItem>();
    }
}
