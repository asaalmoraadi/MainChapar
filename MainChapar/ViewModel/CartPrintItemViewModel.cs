namespace MainChapar.ViewModel
{
    public class CartPrintItemViewModel
    {
        public int Id { get; set; }               // شناسه PickupPrintItem
        public int PrintRequestId { get; set; }  // شناسه درخواست چاپ
        public string ServiceType { get; set; }  // نوع سرویس (Color, BlackWhite, Plan)
        public string Status { get; set; }       // وضعیت درخواست
        public decimal TotalPrice { get; set; }  // قیمت نهایی
        public DateTime CreatedAt { get; set; }  // زمان ایجاد درخواست
    }
}
