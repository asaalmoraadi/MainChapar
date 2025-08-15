using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class ColorPrintDetail
    {
        [Key]
        public int PrintRequestId { get; set; }
        public PrintRequest PrintRequest { get; set; }

        public string PaperType { get; set; } // Glossy, Normal
        public string PaperSize { get; set; } // A4, A5, A3
        public int CopyCount { get; set; }
        public string PrintSide { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalPages { get; set; }
        public string Description { get; set; }
        public string? AdditionalDescription { get; set; } //توضیحات اضافه مثل موقعیت نقشه، حاشیه، یا تنظیمات خاص
        public string BindingType { get; set; } = "هیچ‌کدام"; //منگنه ، طلق و شیرازه ، هیچکدام ، سیمی
        public string? FilesJson { get; set; }  // رشته‌ای که لیست فایل‌ها را به صورت JSON نگه می‌دارد
    }
}
