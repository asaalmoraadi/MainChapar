using MainChapar.Models;

namespace MainChapar.ViewModel
{
    public class CartViewModel
    {
        public List<CartProductItemViewModel> Products { get; set; }
        public List<CartPrintItemViewModel> PrintRequests { get; set; }

        public decimal TotalPrice =>
            PrintRequests.Sum(p => p.TotalPrice) +
            Products.Sum(p => p.TotalPrice);
    }
}
