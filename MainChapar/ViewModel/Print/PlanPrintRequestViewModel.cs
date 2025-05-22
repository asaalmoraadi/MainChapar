namespace MainChapar.ViewModel.Print
{
    public class PlanPrintRequestViewModel
    {
        public IFormFile[] Files { get; set; }
        public string PlanSize { get; set; }
        public int CopyCount { get; set; }
    }
}
