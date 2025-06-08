using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class BlackWhitePrintDetail
    {
        [Key]
        public int PrintRequestId { get; set; }
        public PrintRequest PrintRequest { get; set; }

        public string PaperType { get; set; }
        public string PaperSize { get; set; }
        public string PrintSide { get; set; }
        public int CopyCount { get; set; }
        public int TotalPages { get; set; }
        public string Description { get; set; }
        public string BindingType { get; set; } = "هیچ‌کدام"; //منگنه ، طلق و شیرازه ، هیچکدام ، سیمی
        public string? FilesJson { get; set; }  // رشته‌ای که لیست فایل‌ها را به صورت JSON نگه می‌دارد
        public decimal TotalPrice { get; set; }
    }
}
