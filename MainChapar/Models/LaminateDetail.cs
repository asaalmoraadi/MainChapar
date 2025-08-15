using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class LaminateDetail
    {
        [Key]
        public int PrintRequestId { get; set; }
        public PrintRequest PrintRequest { get; set; }

        public string PaperType { get; set; }
        public string PaperSize { get; set; }
        public string PrintSide { get; set; }
        public string printType { get; set; } //  BW , color
        public int CopyCount { get; set; }
        public int TotalPages { get; set; }
        public string Description { get; set; }
        public string LaminateType { get; set; } = "مات"; //مات ، براق
        public string CornerType { get; set; } //گوشه گرد، گوشه تیز
        public string? FilesJson { get; set; }  // رشته‌ای که لیست فایل‌ها را به صورت JSON نگه می‌دارد
        public decimal TotalPrice { get; set; }
    }
}
