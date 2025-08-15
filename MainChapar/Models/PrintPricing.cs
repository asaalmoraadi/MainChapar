namespace MainChapar.Models
{
    public class PrintPricing
    {

        public int Id { get; set; }
        public string PaperSize { get; set; } // A4, A5, A3
        public string PaperType { get; set; } // ساده، گلاسه سبک، گلاسه سنگین
        public string PrintSide { get; set; }
        public decimal PricePerPage { get; set; }
        public bool IsAvailable { get; set; }
        public string PrintType { get; set; } // "BlackWhite" یا "Color"
        
    }
}
