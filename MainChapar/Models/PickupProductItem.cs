namespace MainChapar.Models
{
    public class PickupProductItem
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        // Navigation to Product
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Navigation to PickupRequest
        public int PickupRequestId { get; set; }
        public PickupRequest PickupRequest { get; set; }

        // محاسبه قیمت نهایی این آیتم
        public decimal TotalPrice => (Product?.Price ?? 0 - Product?.Discount ?? 0) * Quantity;
    }
}
