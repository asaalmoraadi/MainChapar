using MainChapar.Data;
using MainChapar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
             .Where(p => p.IsAvailable==true && p.Qty > 0)
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

            return View(product);
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
