using MainChapar.Models;

namespace MainChapar.ViewModel
{
    public class UserOrdersViewModel
    {
        public List<Order> ProductOrders { get; set; } = new();
        public List<PrintRequest> PrintRequests { get; set; } = new();
    }
}
