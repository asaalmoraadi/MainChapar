using System.ComponentModel.DataAnnotations;

namespace MainChapar.ViewModel
{
    public class UsedBookCreateViewModel
    {
        [Required(ErrorMessage = "عنوان کتاب الزامی است")]
        [StringLength(200, ErrorMessage = "عنوان نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        [Display(Name = "عنوان کتاب")]
        public string Title { get; set; }

        [Required(ErrorMessage = "نام نویسنده الزامی است")]
        [StringLength(150, ErrorMessage = "نام نویسنده نمی‌تواند بیشتر از 150 کاراکتر باشد")]
        [Display(Name = "نویسنده")]
        public string Author { get; set; }

        [Display(Name = "توضیحات")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "قیمت الزامی است")]
        [Range(1000, 100000000, ErrorMessage = "قیمت باید بین ۱۰۰۰ و ۱۰۰ میلیون تومان باشد")]
        [Display(Name = "قیمت (تومان)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "شماره تماس الزامی است")]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره تماس معتبر نیست")]
        [Display(Name = "شماره تماس")]
        public string ContactNumber { get; set; }

        

        [Required(ErrorMessage = "عکس اصلی را انتخاب کنید")]
        [Display(Name = "عکس اصلی کتاب")]
        public IFormFile MainImage { get; set; }   //  عکس اصلی
        public List<IFormFile> Images { get; set; }
    }
}
