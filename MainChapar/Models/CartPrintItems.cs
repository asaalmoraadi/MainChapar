using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class CartPrintItems
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public int PrintRequestId { get; set; }
        public PrintRequest PrintRequest { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}
