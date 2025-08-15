using MainChapar.Models;

namespace MainChapar.ViewModel
{
    public class OrderDetailsViewModel
    {
        public PickupRequest PickupRequest { get; set; }
        public List<PrintRequest> PrintRequests { get; set; }
        public bool IsReadyForPickup { get; set; }
    }
}
