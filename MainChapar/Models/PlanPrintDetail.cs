using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class PlanPrintDetail
    {
        [Key]
        public int PrintRequestId { get; set; }
        public PrintRequest PrintRequest { get; set; }
        public string SizeOrScaleDescription { get; set; }
        public string? AdditionalDescription { get; set; } //توضیحات اضافه مثل موقعیت نقشه، حاشیه، یا تنظیمات خاص
        public string PaperType { get; set; } // Glossy, Normal
        public string printType { get; set; } //  BW , color

        public int CopyCount { get; set; }
        
        public string BindingType { get; set; } = "هیچ‌کدام"; //منگنه ، طلق و شیرازه ، هیچکدام ، سیمی
        public string? FilesJson { get; set; }  // رشته‌ای که لیست فایل‌ها را به صورت JSON نگه می‌دارد
    }
}
