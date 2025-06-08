using MainChapar.Data;
using MainChapar.Models;
using MainChapar.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QRCoder;
using System.Security.Claims;

namespace MainChapar.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CartController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // نمایش سبد خرید - با استفاده از ViewModel جدید
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "ابتدا وارد حساب کاربری شوید.";
                return RedirectToAction("Index", "Home");
            }

            // جستجوی آخرین سبد خرید باز (در انتظار تحویل) کاربر
            var pickupRequest = await _context.pickupRequests
                .Include(p => p.ProductItems)
                    .ThenInclude(pi => pi.Product)
                .Include(p => p.PickupPrintItems)
                    .ThenInclude(ppi => ppi.PrintRequest)
                .Where(p => p.UserId == userId && !p.IsDelivered)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (pickupRequest == null)
            {
                // اگر سبد خالی است ویومدل خالی برمیگردانیم
                return View(new CartViewModel());
            }

            var cartVM = new CartViewModel();

            // تبدیل PickupProductItem به CartProductItemViewModel
            cartVM.Products = pickupRequest.ProductItems.Select(pi => new CartProductItemViewModel
            {
                Id = pi.Id,
                ProductId = pi.ProductId,
                Title = pi.Product.Title,
                ImageName = string.IsNullOrEmpty(pi.Product.ImageName) ? "default.png" : pi.Product.ImageName,
                Quantity = pi.Quantity,
                UnitPrice = (pi.Product.Price - (pi.Product.Discount ?? 0)),
                TotalPrice = (pi.Product.Price - (pi.Product.Discount ?? 0)) * pi.Quantity,
                AvailableStock = pi.Product.Qty
            }).ToList();

            // تبدیل PickupPrintItem به CartPrintItemViewModel
            cartVM.PrintRequests = pickupRequest.PickupPrintItems.Select(pi => new CartPrintItemViewModel
            {
                Id = pi.Id,
                PrintRequestId = pi.PrintRequestId,
                ServiceType = pi.PrintRequest.ServiceType,
                Status = pi.PrintRequest.Status,
                TotalPrice = pi.PrintRequest.TotalPrice ?? 0,
                CreatedAt = pi.PrintRequest.CreatedAt
            }).ToList();

            return View(cartVM);
        }

        // به‌روزرسانی تعداد محصول در سبد
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity < 1)
                quantity = 1;

            var productJson = HttpContext.Session.GetString("CartProducts");
            Dictionary<int, int> cartDict = new Dictionary<int, int>();

            if (!string.IsNullOrEmpty(productJson))
                cartDict = JsonConvert.DeserializeObject<Dictionary<int, int>>(productJson);

            if (cartDict.ContainsKey(productId))
                cartDict[productId] = quantity;

            HttpContext.Session.SetString("CartProducts", JsonConvert.SerializeObject(cartDict));

            return RedirectToAction("Index");
        }

        // ثبت نهایی سبد خرید با ذخیره در دیتابیس و کاهش موجودی محصولات
        [HttpPost]
        public async Task<IActionResult> Submit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "کاربر شناسایی نشد. لطفاً دوباره وارد شوید.";
                return RedirectToAction("Index");
            }

            var printJson = HttpContext.Session.GetString("CartPrints");
            var productJson = HttpContext.Session.GetString("CartProducts");

            if (string.IsNullOrEmpty(printJson) && string.IsNullOrEmpty(productJson))
            {
                TempData["Error"] = "سبد خرید خالی است!";
                return RedirectToAction("Index");
            }

            try
            {
                var pickupRequest = new PickupRequest
                {
                    CreatedAt = DateTime.Now,
                    Status = "در انتظار تأیید",
                    QrCodeToken = Guid.NewGuid().ToString(),
                    UserId = userId
                };

                _context.pickupRequests.Add(pickupRequest);
                await _context.SaveChangesAsync();

                // ذخیره چاپ‌ها
                if (!string.IsNullOrEmpty(printJson))
                {
                    var printIds = JsonConvert.DeserializeObject<List<int>>(printJson);

                    var existingPrints = await _context.PrintRequests
                        .Where(p => printIds.Contains(p.Id))
                        .Select(p => p.Id)
                        .ToListAsync();

                    foreach (var id in existingPrints)
                    {
                        _context.pickupPrintItems.Add(new PickupPrintItem
                        {
                            PrintRequestId = id,
                            PickupRequestId = pickupRequest.Id
                        });
                    }
                }

                // ذخیره محصولات و کاهش موجودی
                if (!string.IsNullOrEmpty(productJson))
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<int, int>>(productJson);

                    var validProductIds = await _context.Products
                        .Where(p => dict.Keys.Contains(p.Id))
                        .Select(p => p.Id)
                        .ToListAsync();

                    foreach (var item in dict)
                    {
                        if (!validProductIds.Contains(item.Key)) continue;
                        if (item.Value <= 0) continue;

                        var product = await _context.Products.FindAsync(item.Key);
                        if (product == null) continue;

                        if (product.Qty < item.Value)
                        {
                            TempData["Error"] = $"موجودی محصول {product.Title} کافی نیست.";
                            return RedirectToAction("Index");
                        }

                        product.Qty -= item.Value;
                        if (product.Qty <= 0)
                            product.IsAvailable = false;

                        _context.pickupProducts.Add(new PickupProductItem
                        {
                            ProductId = item.Key,
                            Quantity = item.Value,
                            PickupRequestId = pickupRequest.Id
                        });
                    }
                }

                await _context.SaveChangesAsync();

                // پاک کردن سشن
                HttpContext.Session.Remove("CartPrints");
                HttpContext.Session.Remove("CartProducts");

                return RedirectToAction("Success", new { id = pickupRequest.Id });
            }
            catch (Exception)
            {
                TempData["Error"] = "خطایی در ثبت سبد خرید رخ داد.";
                return RedirectToAction("Index");
            }
        }

        // نمایش سفارش‌های کاربر
        public async Task<IActionResult> Orders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "کاربر شناسایی نشد. لطفاً وارد شوید.";
                return RedirectToAction("Index", "Home");
            }

            var orders = await _context.pickupRequests
                .Where(o => o.UserId == userId)
                .Include(o => o.ProductItems)
                    .ThenInclude(pi => pi.Product)
                .Include(o => o.PickupPrintItems)
                    .ThenInclude(ppi => ppi.PrintRequest)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // نمایش جزئیات سفارش
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.pickupRequests
                .Where(p => p.Id == id && p.UserId == userId)
                .Include(p => p.ProductItems)
                    .ThenInclude(pi => pi.Product)
                .Include(p => p.PickupPrintItems)
                    .ThenInclude(pp => pp.PrintRequest)
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound();

            return View(order);
        }

        // صفحه موفقیت و تولید QR کد (در صورت آماده بودن سفارش)
        public async Task<IActionResult> Success(int id)
        {
            var request = await _context.pickupRequests
                .Include(r => r.PickupPrintItems)
                    .ThenInclude(pi => pi.PrintRequest)
                .Include(r => r.ProductItems)
                    .ThenInclude(pi => pi.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            bool hasPrints = request.PickupPrintItems?.Any() == true;
            bool allPrintsReady = request.PickupPrintItems?.All(p => p.PrintRequest.Status == "آماده تحویل") ?? false;
            bool hasOnlyProducts = !hasPrints && (request.ProductItems?.Any() == true);

            if ((hasPrints && allPrintsReady) || hasOnlyProducts)
            {
                string qrText = $"PickupRequest:{id}";

                using (var qrGenerator = new QRCodeGenerator())
                using (var qrData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q))
                {
                    var qrCode = new SvgQRCode(qrData);
                    string svgImage = qrCode.GetGraphic(5);
                    ViewBag.QrCode = $"data:image/svg+xml;utf8,{svgImage}";
                }
            }
            else
            {
                ViewBag.QrCode = null;
                ViewBag.Message = "سفارش شما در حال پردازش است و هنوز آماده تحویل نمی‌باشد.";
            }

            ViewBag.RequestId = id;
            return View(request);
        }
    }
}
