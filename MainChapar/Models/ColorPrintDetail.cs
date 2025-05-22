using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class ColorPrintDetail
    {
        [Key]
        public int PrintRequestId { get; set; }
        public PrintRequest PrintRequest { get; set; }

        public string PaperType { get; set; } // Glossy, Normal
        public string PaperSize { get; set; } // A4, A5
        public string PrintSide { get; set; } // Single, Double
        public int CopyCount { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
