namespace MainChapar.ViewModel.Print
{
    public class PlanPrintSummaryViewModel
    {
        public string SizeOrScaleDescription { get; set; }
        public string PaperType { get; set; } // Glossy, Normal
        public string printType { get; set; } //  BW , color
        public List<PrintFileViewModel> Files { get; set; } = new();
        public int CopyCount { get; set; }
        public string? AdditionalDescription { get; set; }
        public string BindingType { get; set; }
    }
}
