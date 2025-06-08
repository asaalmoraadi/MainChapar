using MainChapar.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using System.IO;


namespace MainChapar.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------ لیست همه سفارش‌ها ------------------
        public async Task<IActionResult> Index()
        {
            var orders = await _context.orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // ------------------ نمایش جزئیات یک سفارش ------------------
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // ------------------ تأیید سفارش (ادمین) ------------------
        [HttpPost]
        public async Task<IActionResult> Confirm(int id)
        {
            var order = await _context.orders.FindAsync(id);
            if (order == null) return NotFound();

            order.IsConfirmed = true;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ------------------ علامت‌گذاری به عنوان تحویل شده ------------------
        [HttpPost]
        public async Task<IActionResult> MarkAsCollected(int id)
        {
            var order = await _context.orders.FindAsync(id);
            if (order == null) return NotFound();

            order.IsCollected = true;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

       
    }
}
