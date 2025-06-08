using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        // Navigation property
        public List<Product>? Products { get; set; }
    }
}
