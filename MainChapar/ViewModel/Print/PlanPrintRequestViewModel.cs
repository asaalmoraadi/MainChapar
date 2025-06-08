using System.ComponentModel.DataAnnotations;

namespace MainChapar.ViewModel.Print
{
    public class PlanPrintRequestViewModel
    {
        [Required(ErrorMessage = "لطفاً حداقل یک فایل آپلود کنید.")]
        public IFormFile[] Files { get; set; }

        [Required(ErrorMessage = "تعداد نسخه را وارد کنید.")]
        [Range(1, 100, ErrorMessage = "تعداد نسخه باید بین 1 تا 100 باشد.")]
        public int CopyCount { get; set; }

        [Required(ErrorMessage = "لطفاً اندازه نقشه یا مقیاس را وارد کنید.")]
        [StringLength(200, ErrorMessage = "متن وارد شده بیش از حد طولانی است.")]
        public string SizeOrScaleDescription { get; set; }

        [Required(ErrorMessage = "انتخاب نوع چاپ الزامی است.")]
        public string printType { get; set; } //  BW , color

        [Required(ErrorMessage = "نوع کاغذ الزامی است")]
        public string PaperType { get; set; }

        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.")]
        public string? AdditionalDescription { get; set; } //توضیحات اضافه مثل موقعیت نقشه، حاشیه، یا تنظیمات خاص
        [Display(Name = "نوع صحافی")]
        public string BindingType { get; set; } = "هیچ‌کدام";
    }
}
