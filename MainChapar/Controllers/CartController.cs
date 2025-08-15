using MainChapar.Data;
using MainChapar.Models;
using MainChapar.ViewModel;
using Microsoft.AspNetCore.Authorization;
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

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "ابتدا وارد حساب کاربری شوید.";
                return RedirectToAction("Index", "Home");
            }

            // دریافت آیتم‌های چاپی در سبد خرید که هنوز ثبت نشده‌اند
            var cartPrints = await _context.CartPrintItems
                .Where(c => c.UserId == userId)
                .Include(c => c.PrintRequest)
                .ToListAsync();

            // دریافت آیتم‌های محصولی در سبد خرید که هنوز ثبت نشده‌اند (IsFinalized = false)
            var cartProducts = await _context.CartProductItems
                .Where(c => c.UserId == userId && !c.IsFinalized)
                .Include(c => c.Product)
                .ToListAsync();

            var cartVM = new CartViewModel();

            cartVM.PrintRequests = cartPrints.Select(c => new CartPrintItemViewModel
            {
                Id = c.Id,
                PrintRequestId = c.PrintRequestId,
                ServiceType = c.PrintRequest.ServiceType,
                Status = c.PrintRequest.Status,
                TotalPrice = c.PrintRequest.TotalPrice ?? 0,
                CreatedAt = c.PrintRequest.CreatedAt
            }).ToList();

            cartVM.Products = cartProducts.Select(c => new CartProductItemViewModel
            {
                ProductId = c.ProductId,
                Title = c.Product.Title,
                ImageName = string.IsNullOrEmpty(c.Product.ImageName) ? "default.png" : c.Product.ImageName,
                Quantity = c.Quantity,
                UnitPrice = (c.Product.Price - (c.Product.Discount ?? 0)),
                TotalPrice = (c.Product.Price - (c.Product.Discount ?? 0)) * c.Quantity,
                AvailableStock = c.Product.Qty
            }).ToList();

            return View(cartVM);
        }


        [HttpPost]
        public IActionResult TestSubmit()
        {
            return Content("Submit reached!");
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


        //[HttpPost]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Submit()
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        TempData["Error"] = "کاربر شناسایی نشد. لطفاً دوباره وارد شوید.";
        //        return RedirectToAction("Index");
        //    }

        //    // دریافت آیتم‌های سبد خرید (چاپ‌ها بدون IsFinalized، محصولات با شرط IsFinalized == false)
        //    var cartPrints = await _context.CartPrintItems
        //        .Where(x => x.UserId == userId)
        //        .ToListAsync();

        //    var cartProducts = await _context.CartProductItems
        //        .Where(x => x.UserId == userId && !x.IsFinalized)
        //        .Include(x => x.Product)
        //        .ToListAsync();

        //    if (!cartPrints.Any() && !cartProducts.Any())
        //    {
        //        TempData["Error"] = "سبد خرید خالی است!";
        //        return RedirectToAction("Index");
        //    }

        //    // شروع تراکنش برای جلوگیری از ذخیره ناقص
        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        // اگر سفارش شامل چاپ باشد باید تأیید شود، در غیر این‌صورت مستقیم تایید می‌شود
        //        var status = cartPrints.Any() ? "Processing" : "Approved";

        //        var pickupRequest = new PickupRequest
        //        {
        //            CreatedAt = DateTime.Now,
        //            QrCodeToken = Guid.NewGuid().ToString(),
        //            UserId = userId,
        //            PickupCode = Guid.NewGuid().ToString()
        //        };

        //        _context.pickupRequests.Add(pickupRequest);
        //        await _context.SaveChangesAsync();

        //        // ذخیره چاپ‌ها
        //        foreach (var item in cartPrints)
        //        {
        //            _context.pickupPrintItems.Add(new PickupPrintItem
        //            {
        //                PrintRequestId = item.PrintRequestId,
        //                PickupRequestId = pickupRequest.Id
        //            });
        //        }

        //        // ذخیره محصولات و کاهش موجودی، و علامت‌گذاری به عنوان نهایی شده
        //        foreach (var item in cartProducts)
        //        {
        //            if (item.Quantity <= 0 || item.Product == null) continue;

        //            if (item.Product.Qty < item.Quantity)
        //            {
        //                TempData["Error"] = $"موجودی محصول «{item.Product.Title}» کافی نیست.";
        //                return RedirectToAction("Index");
        //            }

        //            item.Product.Qty -= item.Quantity;
        //            if (item.Product.Qty <= 0)
        //                item.Product.IsAvailable = false;

        //            _context.pickupProducts.Add(new PickupProductItem
        //            {
        //                ProductId = item.ProductId,
        //                Quantity = item.Quantity,
        //                PickupRequestId = pickupRequest.Id
        //            });

        //            // علامت‌گذاری آیتم به عنوان نهایی شده
        //            item.IsFinalized = true;
        //        }

        //        // حذف آیتم‌های چاپ از CartPrintItems پس از ثبت سفارش
        //        _context.CartPrintItems.RemoveRange(cartPrints);

        //        // ذخیره همه تغییرات (محصولات، موجودی، حذف چاپ‌ها، نهایی‌سازی محصولات)
        //        await _context.SaveChangesAsync();

        //        await transaction.CommitAsync();

        //        return RedirectToAction("Index", "UserOrder");
        //    }
        //    catch (Exception)
        //    {
        //        await transaction.RollbackAsync();
        //        TempData["Error"] = "خطایی در ثبت سبد خرید رخ داد.";
        //        return RedirectToAction("Index");
        //    }
        //}

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "کاربر شناسایی نشد. لطفاً دوباره وارد شوید.";
                return RedirectToAction("Index");
            }

            //var cartPrints = await _context.CartPrintItems
            //    .Where(x => x.UserId == userId)
            //    .ToListAsync();

            //بخش جدید
            var cartPrints = await _context.CartPrintItems
                .Where(x => x.UserId == userId)
                .Include(x => x.PrintRequest)
                 .ToListAsync();
            //
            var cartProducts = await _context.CartProductItems
                .Where(x => x.UserId == userId && !x.IsFinalized)
                .Include(x => x.Product)
                .ToListAsync();

            TempData["DebugCartPrintsCount"] = cartPrints.Count;
            TempData["DebugCartProductsCount"] = cartProducts.Count;

            if (!cartPrints.Any() && !cartProducts.Any())
            {
                TempData["Error"] = "سبد خرید خالی است!";
                return RedirectToAction("Index");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var pickupRequest = new PickupRequest
                {
                    CreatedAt = DateTime.Now,
                    QrCodeToken = Guid.NewGuid().ToString(),
                    PickupCode = Guid.NewGuid().ToString(),
                    UserId = userId
                };

                _context.pickupRequests.Add(pickupRequest);
                await _context.SaveChangesAsync();

                

                foreach (var item in cartPrints)
                {
                    _context.pickupPrintItems.Add(new PickupPrintItem
                    {
                        PickupRequestId = pickupRequest.Id,
                        PrintRequestId = item.PrintRequestId
                    });

                    //بخش جدید
                    // تغییر وضعیت و نهایی‌سازی
                    if (item.PrintRequest != null)
                    {
                        item.PrintRequest.IsFinalized = true;
                        item.PrintRequest.Status = "Processing";
                    }
                }

                foreach (var item in cartProducts)
                {
                    if (item.Quantity <= 0 || item.Product == null)
                        continue;

                    if (item.Product.Qty < item.Quantity)
                    {
                        TempData["Error"] = $"موجودی محصول «{item.Product.Title}» کافی نیست.";
                        return RedirectToAction("Index");
                    }

                    item.Product.Qty -= item.Quantity;
                    if (item.Product.Qty <= 0)
                        item.Product.IsAvailable = false;

                    _context.pickupProducts.Add(new PickupProductItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        PickupRequestId = pickupRequest.Id
                    });

                    item.IsFinalized = true;
                    Console.WriteLine($"ItemId: {item.Id}, IsFinalized: {item.IsFinalized}");
                }

                _context.CartPrintItems.RemoveRange(cartPrints);
                if (cartProducts.Any())
                {
                    var order = new Order
                    {
                        CreatedAt = DateTime.Now,
                        UserId = userId,
                        IsConfirmed = true,
                        IsCollected = false,
                        PickupCode = pickupRequest.PickupCode,
                        OrderDetails = cartProducts.Select(cp => new OrderDetail
                        {
                            ProductId = cp.ProductId,
                            Quantity = cp.Quantity,
                            UnitPrice = cp.Product.Price
                        }).ToList()
                    };

                    _context.orders.Add(order);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("Orders", "Cart");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "خطایی در ثبت سفارش رخ داد.";
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
                        .ThenInclude(pr => pr.BlackWhitePrintDetail)
                .Include(o => o.PickupPrintItems)
                    .ThenInclude(ppi => ppi.PrintRequest)
                        .ThenInclude(pr => pr.ColorPrintDetail)
                .Include(o => o.PickupPrintItems)
                    .ThenInclude(ppi => ppi.PrintRequest)
                        .ThenInclude(pr => pr.PlanPrintDetail)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.pickupRequests
                .Where(p => p.Id == id && p.UserId == userId)
                .Include(p => p.ProductItems)
                    .ThenInclude(pi => pi.Product)
                .Include(p => p.PickupPrintItems)
                    .ThenInclude(pp => pp.PrintRequest)
                        .ThenInclude(pr => pr.BlackWhitePrintDetail)
                .Include(p => p.PickupPrintItems)
                    .ThenInclude(pp => pp.PrintRequest)
                        .ThenInclude(pr => pr.ColorPrintDetail)
                .Include(p => p.PickupPrintItems)
                    .ThenInclude(pp => pp.PrintRequest)
                        .ThenInclude(pr => pr.PlanPrintDetail)
                .Include(p => p.PickupPrintItems)
                    .ThenInclude(pp => pp.PrintRequest)
                        .ThenInclude(pr => pr.LaminateDetail)
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound();

            // لیست چاپ‌ها
            var printRequests = order.PickupPrintItems?
                .Where(p => p.PrintRequest != null)
                .Select(p => p.PrintRequest)
                .ToList() ?? new List<PrintRequest>();

            // بررسی آمادگی تحویل
            bool hasPrints = order.PickupPrintItems?.Any() == true;
            bool hasProducts = order.ProductItems?.Any() == true;

            var notRejectedPrints = order.PickupPrintItems?
                .Where(p => p.PrintRequest.Status != "Rejected")
                .ToList();

            bool allNonRejectedPrintsCompleted = notRejectedPrints?
                .All(p => p.PrintRequest.Status == "Completed") ?? true;

            bool isReady = (!hasPrints && hasProducts) || (hasPrints && allNonRejectedPrintsCompleted);

            // ساخت ViewModel
            var viewModel = new OrderDetailsViewModel
            {
                PickupRequest = order,
                PrintRequests = printRequests,
                IsReadyForPickup = isReady
            };

            return View(viewModel);
        }

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
            bool hasProducts = request.ProductItems?.Any() == true;

            // فقط درخواست‌هایی که رد نشده‌اند بررسی شوند
            var notRejectedPrints = request.PickupPrintItems?
                .Where(p => p.PrintRequest.Status != "Rejected")
                .ToList();

            // اگر همه‌ی موارد غیرردشده Completed باشند، آنگاه آماده تحویل است
            bool allNonRejectedPrintsCompleted = notRejectedPrints?
                .All(p => p.PrintRequest.Status == "Completed") ?? true;

            // شرایط آماده بودن سفارش:
            // - اگر فقط محصول باشد
            // - اگر چاپ هم باشد ولی همه نهایی شده باشند (تکمیل برای موارد تأییدشده)
            bool isReady = (!hasPrints && hasProducts) || (hasPrints && allNonRejectedPrintsCompleted);

            if (!isReady)
            {
                TempData["Error"] = "سفارش شما هنوز آماده تحویل نیست.";
                return RedirectToAction("UserOrder", "User");
            }

            if (string.IsNullOrEmpty(request.QrCodeToken))
            {
                request.QrCodeToken = Guid.NewGuid().ToString();
                await _context.SaveChangesAsync();
            }

            string qrText = $"PickupRequest:{request.QrCodeToken}";

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q))
            {
                var qrCode = new SvgQRCode(qrData);
                string svgImage = qrCode.GetGraphic(5);
                ViewBag.QrCode = $"data:image/svg+xml;utf8,{svgImage}";
            }

            ViewBag.RequestId = id;
            ViewBag.Message = "سفارش شما آماده تحویل است. لطفاً کد QR را به اپراتور نشان دهید.";

            return View(request);
        }



        //حذف محصول
        public enum CartItemType
        {
            Product,
            Print
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int itemId, CartItemType itemType)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index");

            switch (itemType)
            {
                case CartItemType.Product:
                    var cartProduct = await _context.CartProductItems
                        .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == itemId && !c.IsFinalized);
                    if (cartProduct != null)
                    {
                        _context.CartProductItems.Remove(cartProduct);
                        await _context.SaveChangesAsync();
                    }
                    break;

                case CartItemType.Print:
                    var cartPrint = await _context.CartPrintItems
                        .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == itemId);
                    if (cartPrint != null)
                    {
                        _context.CartPrintItems.Remove(cartPrint);
                        await _context.SaveChangesAsync();
                    }
                    break;
            }

            return RedirectToAction("Index");
        }



    }
}
