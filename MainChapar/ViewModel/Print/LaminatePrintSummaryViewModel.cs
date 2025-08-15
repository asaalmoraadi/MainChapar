namespace MainChapar.ViewModel.Print
{
    public class LaminatePrintSummaryViewModel
    {
        public string PaperType { get; set; }
        public string PaperSize { get; set; }
        public int CopyCount { get; set; }
        public string PrintSide { get; set; }
        public List<PrintFileViewModel> Files { get; set; } = new();
        public int TotalPages { get; set; }
        public decimal TotalPrice { get; set; }

        public string Description { get; set; }
        public string printType { get; set; } // BW , color
        public string LaminateType { get; set; } //مات ، براق
        public string CornerType { get; set; } //گوشه گرد، گوشه تیز
    }
}
