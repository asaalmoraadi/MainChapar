using MainChapar.Data;
using MainChapar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MainChapar.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ProductController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
           
            var products = await _context.Products
             .Where(p => p.IsAvailable == true && p.Qty > 0)
             .OrderByDescending(p => p.CreatedAt)  // مرتب بر اساس تاریخ ایجاد، جدیدترین‌ها
             .Take(8)                               // فقط ۵ تا اول
             .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductGalleries)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // دریافت نظرات این محصول
            var comments = await _context.comments
                .Where(c => c.ProductId == id)
                .Include(c => c.User)
                .ToListAsync();

            ViewBag.Comments = comments;

            // دریافت محصولات مشابه (بر اساس CategoryId مثلاً)
            var related = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(3)
                .ToListAsync();

            ViewBag.RelatedProducts = related ?? new List<Product>();

            return View(product);
        }

        public IActionResult Category(string categorySlug)
        {
            if (string.IsNullOrEmpty(categorySlug))
                return NotFound();

            var category = _context.categories.FirstOrDefault(c => c.Slug == categorySlug);
            if (category == null)
                return NotFound();

            ViewBag.CategoryName = category.Name;
            ViewBag.CategorySlug = categorySlug;

            return View(); // فقط ویوی صفحه اصلی
        }

        [HttpGet]
        public IActionResult FilterProducts(string categorySlug, string sort)
        {
            var category = _context.categories.FirstOrDefault(c => c.Slug == categorySlug);
            if (category == null)
                return NotFound();

            var productsQuery = _context.Products
                .Where(p => p.CategoryId == category.Id && p.IsAvailable)
                .Include(p => p.ProductGalleries)
                .AsQueryable();

            switch (sort)
            {
                case "mostDiscount":
                    productsQuery = productsQuery.OrderByDescending(p => p.Discount ?? 0);
                    break;
                case "mostStock":
                    productsQuery = productsQuery.OrderByDescending(p => p.Qty);
                    break;
                case "expensive":
                    productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                case "cheap":
                    productsQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                case "newest":
                    productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
                    break;
                default:
                    productsQuery = productsQuery.OrderByDescending(p => p.Id);
                    break;
            }

            return PartialView("_ProductListPartial", productsQuery.ToList());
        }



        [HttpGet]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                // اگر چیزی وارد نشده، همه یا هیچ محصولی نشان بده
                var allProducts = _context.Products.ToList();
                return View("SearchResults", allProducts);
            }

            // جستجو در نام محصولات (یا هر فیلدی که میخوای)
            var results = _context.Products
                .Where(p => p.Title.Contains(query))
                .ToList();

            return View("Search", results);
        }

        //ارسال به سبد خرید
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            // حداقل تعداد باید 1 باشد
            if (quantity < 1)
                quantity = 1;

            // دریافت محصول از دیتابیس با توجه به شناسه
            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsAvailable || product.Qty < quantity)
            {
                // اگر محصول یافت نشد یا موجود نیست یا تعداد درخواست شده بیشتر از موجودی است
                ModelState.AddModelError("", "محصول انتخاب شده موجود نیست یا تعداد درخواستی معتبر نیست.");
                // بازگشت به صفحه جزئیات محصول برای نمایش خطا
                return RedirectToAction("Details", new { id = productId });
            }

            // گرفتن شناسه کاربر لاگین کرده
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                // اگر کاربر لاگین نکرده است، هدایت به صفحه ورود
                return RedirectToAction("Login", "User");
            }

           
            // بررسی اینکه آیا قبلاً این محصول در سبد خرید هست یا نه
            var existingItem = await _context.CartProductItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId && !c.IsFinalized);


            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartProductItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };
                _context.CartProductItems.Add(cartItem);
            }
            // ذخیره تغییرات در دیتابیس
            await _context.SaveChangesAsync();

        
            // هدایت به صفحه سبد خرید برای مشاهده محتویات
            return RedirectToAction("Index", "Cart");
        }




        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> AddComment(int productId, string text)
        {
            var userId = _userManager.GetUserId(User);

            var comment = new Comment
            {
                ProductId = productId,
                Text = text,
                UserId = userId
            };

            _context.comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = productId });
        }


    }
}
