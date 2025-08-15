using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Slug { get; set; }         // انگلیسی، برای URL

        // Navigation property
        public List<Product>? Products { get; set; }
    }
}
