namespace MainChapar.Models
{
    public class PickupPrintItem
    {
        public int Id { get; set; }

        public int PrintRequestId { get; set; }
        public PrintRequest PrintRequest { get; set; }

        public int PickupRequestId { get; set; }
        public PickupRequest PickupRequest { get; set; }

        // محاسبه قیمت نهایی بر اساس PrintRequest
        public decimal TotalPrice => PrintRequest?.TotalPrice ?? 0;
    }
}
