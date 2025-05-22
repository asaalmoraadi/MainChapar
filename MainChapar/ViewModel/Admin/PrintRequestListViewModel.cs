namespace MainChapar.ViewModel.Admin
{
    public class PrintRequestListViewModel
    {
        public int Id { get; set; }
        public string UserFullName { get; set; }
        public string ServiceType { get; set; }  // رنگی، سیاه‌وسفید، پلان
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }       // Submitted, Processing, Rejected, Completed
        public decimal TotalPrice { get; set; }
    }
}
