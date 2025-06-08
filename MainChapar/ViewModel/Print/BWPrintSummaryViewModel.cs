using MainChapar.Models;
using System.ComponentModel.DataAnnotations;

namespace MainChapar.ViewModel.Print
{
    public class BWPrintSummaryViewModel
    {
        public string PaperType { get; set; }
        public string PaperSize { get; set; }
        public string PrintSide { get; set; }
        public int CopyCount { get; set; }
        public string Description { get; set; }
        public string BindingType { get; set; }
        public List<PrintFileViewModel> Files { get; set; } = new();
        public int TotalPages { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
