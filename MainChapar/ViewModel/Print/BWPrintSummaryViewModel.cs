using MainChapar.Models;

namespace MainChapar.ViewModel.Print
{
    public class BWPrintSummaryViewModel:BasePrintViewModel
    {
        public List<PrintFile> Files { get; set; } = new();
        public int TotalPages { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
