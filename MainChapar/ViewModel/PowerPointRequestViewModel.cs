using System.ComponentModel.DataAnnotations;

namespace MainChapar.ViewModel
{
    public class PowerPointRequestViewModel
    {
        [Required(ErrorMessage = "نام الزامی است")]
        public string Name { get; set; }

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        public string Family { get; set; }

        [Required(ErrorMessage = "شماره تماس الزامی است")]
        [Phone(ErrorMessage = "شماره تماس معتبر نیست")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "تاریخ تحویل الزامی است")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }

        public string Description { get; set; }

        public IFormFile? UploadedFile { get; set; }
        public string? ProjectTitle { get; set; }  // فقط برای پاورپوینت استفاده میشه
    }
}
