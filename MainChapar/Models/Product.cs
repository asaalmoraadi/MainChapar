namespace MainChapar.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? FullDesc { get; set; }

        public decimal? Price { get; set; }

        public decimal? Discount { get; set; }

        public string? ImageName { get; set; }

        public int? Qty { get; set; }

        public string? Tags { get; set; }

        public bool IsAvailable { get; set; }

        public virtual ICollection<ProductGallery> ProductGalleries { get; set; } = new List<ProductGallery>();


    }
}
