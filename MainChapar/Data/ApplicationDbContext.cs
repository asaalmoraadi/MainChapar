using MainChapar.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace MainChapar.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<Banner> Banners { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<Comment> comments { get; set; }

        public DbSet<PrintRequest> PrintRequests { get; set; }
        public DbSet<BlackWhitePrintDetail> BlackWhitePrintDetails { get; set; }
        public DbSet<ColorPrintDetail> ColorPrintDetails { get; set; }
        public DbSet<PlanPrintDetail> PlanPrintDetails { get; set; }
        public DbSet<PrintFile> PrintFiles { get; set; }

        public DbSet<PrintPricing> printPricings { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductGallery> ProductGallerys { get; set; }
    }
}
