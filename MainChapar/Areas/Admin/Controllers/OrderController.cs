using MainChapar.Data;
using MainChapar.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainChapar.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }


        // ------------------ لیست همه سفارشات (PickupRequests) ------------------
        public async Task<IActionResult> Index()
        {
            var pickupRequests = await _context.pickupRequests
            .Include(p => p.User)
            .Include(p => p.ProductItems).ThenInclude(pi => pi.Product)
            .Include(p => p.PickupPrintItems).ThenInclude(pi => pi.PrintRequest)
            .Where(p => p.ProductItems.Any()) // فقط سفارش‌هایی که محصول دارن
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

            return View(pickupRequests);

        }

        // ------------------ جزئیات یک سفارش PickupRequest ------------------
        public async Task<IActionResult> Details(int id)
        {
            var pickup = await _context.pickupRequests
            .Include(p => p.User)
            .Include(p => p.ProductItems).ThenInclude(p => p.Product)
            .Include(p => p.PickupPrintItems).ThenInclude(p => p.PrintRequest)
            .FirstOrDefaultAsync(p => p.Id == id);

            if (pickup == null) return NotFound();

            return View(pickup);
        }

        // ------------------ تأیید سفارش (در صورت نیاز) ------------------
        [HttpPost]
        public async Task<IActionResult> Confirm(int id)
        {
            var pickup = await _context.pickupRequests.FindAsync(id);
            if (pickup == null) return NotFound();

            pickup.IsDelivered = true;
            _context.Update(pickup);
            await _context.SaveChangesAsync();
            //_pickupService.UpdatePickupRequestStatus(pickup.Id);

            TempData["Success"] = "سفارش با موفقیت تأیید شد.";
            return RedirectToAction("Index");
        }

        // ------------------ علامت‌گذاری به عنوان تحویل شده ------------------
        [HttpPost]
        public async Task<IActionResult> MarkAsCollected(int id)
        {
            var pickup = await _context.pickupRequests.FindAsync(id);
            if (pickup == null) return NotFound();

            pickup.IsDelivered = true;
            _context.Update(pickup);
            await _context.SaveChangesAsync();
            //_pickupService.UpdatePickupRequestStatus(pickup.Id);

            TempData["Success"] = "سفارش به عنوان تحویل‌شده علامت‌گذاری شد.";
            return RedirectToAction("Index");
        }

        // ------------------ جستجو بر اساس PickupCode ------------------
        [HttpGet]
        public async Task<IActionResult> FindByCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                TempData["Error"] = "کد وارد نشده است.";
                return RedirectToAction("Index");
            }

            var pickup = await _context.pickupRequests
                .Include(p => p.User)
                .Include(p => p.ProductItems).ThenInclude(pi => pi.Product)
                .Include(p => p.PickupPrintItems).ThenInclude(pi => pi.PrintRequest)
                .FirstOrDefaultAsync(p => p.PickupCode == code);

            if (pickup == null)
            {
                TempData["Error"] = "سفارشی با این کد پیدا نشد.";
                return RedirectToAction("Index");
            }

            return View("VerifyPickup", pickup); // نمایی برای تأیید تحویل
        }

        // ------------------ تأیید تحویل از طریق کد ------------------
        [HttpPost]
        public async Task<IActionResult> ConfirmByCode(int id)
        {
            var pickup = await _context.pickupRequests.FindAsync(id);
            if (pickup == null) return NotFound();

            if (pickup.IsDelivered)
            {
                TempData["Error"] = "این سفارش قبلاً تحویل شده است.";
                return RedirectToAction("FindByCode", new { code = pickup.PickupCode });
            }

            pickup.IsDelivered = true;
            _context.Update(pickup);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تحویل با موفقیت ثبت شد.";
            return RedirectToAction("Index");
        }
    }
}
