using System.ComponentModel.DataAnnotations;

namespace MainChapar.ViewModel.Admin
{
    public class LaminatePricingCreateViewModel
    {
        [Required]
        public string PaperSize { get; set; }

        [Required]
        public string PaperType { get; set; }

        [Required(ErrorMessage = "نوع چاپ الزامی است")]
        public string PrintSide { get; set; }

        [Required]
        public decimal PricePerPage { get; set; }

        [Required]
        public string PrintType { get; set; } // "BlackWhite" یا "Color"

        [Required]
        public string LaminateType { get; set; } // مات یا براق

        public bool IsAvailable { get; set; } = true;
    }
}
