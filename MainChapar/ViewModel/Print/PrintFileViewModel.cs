namespace MainChapar.ViewModel.Print
{
    public class PrintFileViewModel
    {
        public string FileName { get; set; }        // اسم فایل اصلی (مثلاً project.pdf)
        public string FilePath { get; set; }        // مسیر فایل ذخیره‌شده در سرور (برای دانلود یا نمایش)
        public int PageCount { get; set; }          // تعداد صفحات فایل (برای محاسبه قیمت)
        public decimal? FilePrice { get; set; }      // هزینه چاپ همین فایل خاص
    }
}
