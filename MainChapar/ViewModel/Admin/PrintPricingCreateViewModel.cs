using System.ComponentModel.DataAnnotations;

namespace MainChapar.ViewModel.Admin
{
    public class PrintPricingCreateViewModel
    {
        [Required(ErrorMessage = "نوع چاپ را انتخاب کنید.")]
        public string PrintType { get; set; }

        [Required(ErrorMessage = "نوع کاغذ را انتخاب کنید.")]
        public string PaperType { get; set; }

        [Required(ErrorMessage = "سایز کاغذ را انتخاب کنید.")]
        public string PaperSize { get; set; }

        public bool IsDoubleSided { get; set; }

        [Required(ErrorMessage = "قیمت به ازای هر صفحه را وارد کنید.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "قیمت باید عدد مثبت باشد.")]
        public decimal PricePerPage { get; set; }

        public bool IsAvailable { get; set; }

        // برای لیست‌ها:
        //public List<string> PrintTypes { get; set; }
        //public List<string> PaperTypes { get; set; }
        //public List<string> PaperSizes { get; set; }

    }
}
