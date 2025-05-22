using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class PrintRequest
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } // اتصال به Identity
        public User User { get; set; }

        public string ServiceType { get; set; } // Color, BlackWhite, Plan

        public string Status { get; set; } = "Submitted"; // Submitted, Processing, Completed, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public decimal TotalPrice { get; set; }

        public ColorPrintDetail ColorPrintDetail { get; set; }
        public BlackWhitePrintDetail BlackWhitePrintDetail { get; set; }
        public PlanPrintDetail PlanPrintDetail { get; set; }

        public ICollection<PrintFile> Files { get; set; }
    }
}
