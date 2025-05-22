using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class BlackWhitePrintDetail
    {
        [Key]
        public int PrintRequestId { get; set; }
        public PrintRequest PrintRequest { get; set; }

        public string PaperType { get; set; }
        public string PaperSize { get; set; }
        public string PrintSide { get; set; }
        public string ContentType { get; set; } // Text یا Image
        public int CopyCount { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
