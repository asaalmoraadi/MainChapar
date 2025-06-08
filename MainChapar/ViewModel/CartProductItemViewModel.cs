namespace MainChapar.ViewModel
{
    public class CartProductItemViewModel
    {
        public int Id { get; set; }           // شناسه PickupProductItem
        public int ProductId { get; set; }    // شناسه محصول
        public string Title { get; set; }     // عنوان محصول
        public string ImageName { get; set; } // تصویر محصول (برای نمایش)
        public int Quantity { get; set; }     // تعداد سفارش داده شده
        public decimal UnitPrice { get; set; } // قیمت واحد پس از تخفیف (Price - Discount)
        public decimal TotalPrice { get; set; } // قیمت کل (UnitPrice * Quantity)
        public int AvailableStock { get; set; } // موجودی انبار (برای اطلاع کاربر)

    }
}
