using MainChapar.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainChapar.Areas.Admin.Controllers
{
    public class DeliveryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeliveryController(ApplicationDbContext context)
        {
            _context = context;
        }
        // بررسی QR
        [HttpGet]
        public async Task<IActionResult> VerifyQr(string token)
        {
            var request = await _context.pickupRequests
                .Include(r => r.PickupPrintItems).ThenInclude(p => p.PrintRequest)
                .Include(r => r.ProductItems).ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(r => r.QrCodeToken == token);

            if (request == null)
                return NotFound();

            return View(request);
        }

        // تأیید تحویل
        [HttpPost]
        public async Task<IActionResult> ConfirmDelivery(int id)
        {
            var request = await _context.pickupRequests.FindAsync(id);

            if (request == null)
                return NotFound();

            request.IsDelivered = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "تحویل نهایی با موفقیت ثبت شد.";
            return RedirectToAction("VerifyQr"); // یا هر جای مناسب دیگر
        }
    }
}
