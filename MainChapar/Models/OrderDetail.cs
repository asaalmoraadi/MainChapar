namespace MainChapar.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        //nav to Order
        public int OrderId { get; set; }
        public Order Order { get; set; }

        //nav to Product
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
