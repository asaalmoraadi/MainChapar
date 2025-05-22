namespace MainChapar.ViewModel.Print
{
    public class ColorPrintRequestViewModel
    {
        public IFormFile[] Files { get; set; }
        public string PaperSize { get; set; }
        public string PaperType { get; set; }
        public string PrintSide { get; set; }
        public int CopyCount { get; set; }
        public List<string> PaperTypes { get; set; }  // لیست انواع کاغذ
        public List<string> PaperSizes { get; set; }  // لیست اندازه‌های کاغذ
        public List<string> PrintSides { get; set; }  // لیست نوع چاپ
    }
}
