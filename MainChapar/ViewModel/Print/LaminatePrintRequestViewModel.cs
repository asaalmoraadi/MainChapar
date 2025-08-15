using MainChapar.Helpers;
using System.ComponentModel.DataAnnotations;

namespace MainChapar.ViewModel.Print
{
    public class LaminatePrintRequestViewModel
    {
        [Required(ErrorMessage = "لطفاً حداقل یک فایل آپلود کنید.")]
        public IFormFile[] Files { get; set; }

        [Required(ErrorMessage = "تعداد نسخه را وارد کنید.")]
        [Range(1, 100, ErrorMessage = "تعداد نسخه باید بین 1 تا 100 باشد.")]
        public int CopyCount { get; set; }

        [Required(ErrorMessage = "لطفاً اندازه نقشه یا مقیاس را وارد کنید.")]
        [StringLength(200, ErrorMessage = "متن وارد شده بیش از حد طولانی است.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "انتخاب نوع چاپ الزامی است.")]
        public string printType { get; set; } //  BW , color

        [Required(ErrorMessage = "نوع کاغذ الزامی است")]
        public string PaperType { get; set; }

        [Display(Name = "نوع لمینیت")]
        public string LaminateType { get; set; } = "مات"; //مات ، براق
        [Display(Name = "نوع گوشه")]
        public string CornerType { get; set; } //گوشه گرد، گوشه تیز

        [Required(ErrorMessage = "اندازه کاغذ الزامی است")]
        public string PaperSize { get; set; }

        [Required(ErrorMessage = "انتخاب وجه چاپ الزامی است")]
        public string PrintSide { get; set; }

    }
}
