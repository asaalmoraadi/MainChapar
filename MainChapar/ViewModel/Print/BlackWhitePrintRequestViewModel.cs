using MainChapar.Helpers;
using System.ComponentModel.DataAnnotations;

namespace MainChapar.ViewModel.Print
{
    public class BlackWhitePrintRequestViewModel
    {

        [Required(ErrorMessage = "نوع کاغذ الزامی است")]
        public string PaperType { get; set; }

        [Required(ErrorMessage = "اندازه کاغذ الزامی است")]
        public string PaperSize { get; set; }

        [Required(ErrorMessage = "نوع چاپ الزامی است")]
        public string PrintSide { get; set; }

        [Required(ErrorMessage = "تعداد کپی الزامی است")]
        [Range(1, 1000, ErrorMessage = "تعداد کپی باید بین 1 تا 1000 باشد")]
        public int CopyCount { get; set; }

        [Display(Name = "توضیحات برای اپراتور")]
        public string Description { get; set; }

        [Display(Name = "نوع صحافی")]
        public string BindingType { get; set; } = "هیچ‌کدام";

        [AtLeastOneFileRequired(ErrorMessage = "حداقل یک فایل باید انتخاب شود")]
        public List<IFormFile> Files { get; set; }
    }
}
