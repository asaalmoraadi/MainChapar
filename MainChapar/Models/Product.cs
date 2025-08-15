using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "لطفاً عنوان را وارد کنید")]
        public string Title { get; set; }

        public string? Description { get; set; }

        public string? FullDesc { get; set; }

        [Required(ErrorMessage = "لطفاً قیمت را وارد کنید")]
        public decimal Price { get; set; }

        public decimal? Discount { get; set; }

        public string? ImageName { get; set; }

        [Required(ErrorMessage = "موجودی انبار را وارد کنید")]
        public int Qty { get; set; }

        public string? Tags { get; set; }

        public bool IsAvailable { get; set; }

        public DateTime CreatedAt {  get; set; }= DateTime.Now;

        public virtual ICollection<ProductGallery> ProductGalleries { get; set; } = new List<ProductGallery>();


        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
