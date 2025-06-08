using MainChapar.Data;
using MainChapar.Models;
using MainChapar.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace MainChapar.Controllers
{
    public class UserOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserOrderController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // نمایش همه سفارش‌های کاربر جاری (محصولات + خدمات چاپی)
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // سفارش‌های فروشگاهی (محصولات)
            var productOrders = await _context.orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // سفارش‌های خدمات چاپی
            var printRequests = await _context.PrintRequests
                .Where(r => r.UserId == userId)
                .Include(r => r.ColorPrintDetail)
                .Include(r => r.BlackWhitePrintDetail)
                .Include(r => r.PlanPrintDetail)
                .Include(r => r.Files)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();


            var vm = new UserOrdersViewModel
            {
                ProductOrders = productOrders,
                PrintRequests = printRequests
            };


            return View(vm);

        }

        // جزئیات یک سفارش فروشگاهی (اختیاری)
        public async Task<IActionResult> ProductOrderDetails(int id)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.orders
                .Where(o => o.Id == id && o.UserId == userId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();

            return View(order);
        }

        // تولید QR کد برای تحویل حضوری
        public IActionResult GenerateQr(string pickupCode)
        {
            if (string.IsNullOrEmpty(pickupCode)) return NotFound();

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(pickupCode, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);

            return File(qrCodeBytes, "image/png");
        }
    }
}
