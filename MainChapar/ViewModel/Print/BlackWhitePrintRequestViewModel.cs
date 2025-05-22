namespace MainChapar.ViewModel.Print
{
    public class BlackWhitePrintRequestViewModel:BasePrintViewModel
    {
        
        public IFormFile[] Files { get; set; }
 
        // اضافه کردن لیست‌ها برای انتخاب‌ها
        public List<string> PaperTypes { get; set; }  // لیست انواع کاغذ
        public List<string> PaperSizes { get; set; }  // لیست اندازه‌های کاغذ
        public List<string> PrintSides { get; set; }  // لیست نوع چاپ
    }
}
