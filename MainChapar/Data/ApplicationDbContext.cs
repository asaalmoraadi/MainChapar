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
      

        //print
        public DbSet<PrintRequest> PrintRequests { get; set; }
        public DbSet<BlackWhitePrintDetail> BlackWhitePrintDetails { get; set; }
        public DbSet<ColorPrintDetail> ColorPrintDetails { get; set; }
        public DbSet<PlanPrintDetail> PlanPrintDetails { get; set; }
        public DbSet<LaminateDetail> laminateDetails { get; set; }
        public DbSet<PrintFile> PrintFiles { get; set; }
        public DbSet<PrintPricing> printPricings { get; set; }
        public DbSet<LaminatePrintPricing> laminatePrintPricings { get; set; }
        public DbSet<CartPrintItems> CartPrintItems { get; set; }

        //product
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductGallery> ProductGallerys { get; set; }
        public DbSet<Comment> comments { get; set; }
        public DbSet<Category> categories { get; set; }
        public DbSet<Order> orders { get; set; }
        public DbSet<OrderDetail> ordersDetail { get; set; }
        public DbSet<PickupRequest> pickupRequests { get; set; }
        public DbSet< PickupProductItem> pickupProducts { get; set; }
        public DbSet<PickupPrintItem> pickupPrintItems { get; set; }
        public DbSet<CartProductItem> CartProductItems { get; set; }

        public DbSet<Contact> contacts { get; set; }
        public DbSet<Collaboration> collaborations { get; set; }
        public DbSet<UploadedFile> uploadedFiles { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<UsedBook> usedBooks { get; set; }
        public DbSet<UsedBookImage> usedBookImages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PickupPrintItem>()
                .HasOne(p => p.PickupRequest)
                .WithMany(pr => pr.PickupPrintItems)
                .HasForeignKey(p => p.PickupRequestId)
                .OnDelete(DeleteBehavior.Restrict);  // حذف Cascade اینجا غیرفعال شده

            modelBuilder.Entity<PickupPrintItem>()
                .HasOne(p => p.PrintRequest)
                .WithMany(pr => pr.PickupPrintItems)
                .HasForeignKey(p => p.PrintRequestId)
                .OnDelete(DeleteBehavior.Cascade);  // حذف Cascade فقط روی این رابطه فعال است
        }
        public DbSet<MainChapar.Models.UsedBook> UsedBook { get; set; } = default!;
    }
}
