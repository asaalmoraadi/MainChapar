using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class PlanPrintDetail
    {
        [Key]
        public int PrintRequestId { get; set; }
        public PrintRequest PrintRequest { get; set; }

        public string Description { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
