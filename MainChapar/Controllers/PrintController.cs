using MainChapar.Data;
using MainChapar.Models;
using MainChapar.Models.DTO;
using MainChapar.ViewModel.Print;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Pdf.IO;
using System.Security.Claims;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace MainChapar.Controllers
{
    public class PrintController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;

        public PrintController(ApplicationDbContext context, IWebHostEnvironment env, UserManager<User> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: نمایش فرم چاپ سیاه‌وسفید
        [HttpGet]
        public IActionResult BlackWhitePrintForm()
        {
            var vm = new BlackWhitePrintRequestViewModel();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlackWhitePrintForm(BlackWhitePrintRequestViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var pricing = await _context.printPricings.FirstOrDefaultAsync(p =>
                p.PaperType == vm.PaperType &&
                p.PaperSize == vm.PaperSize &&
                p.IsDoubleSided == string.Equals(vm.PrintSide.Trim(), "پشت و رو", StringComparison.OrdinalIgnoreCase) &&
                p.IsAvailable);

            if (pricing == null)
            {
                ModelState.AddModelError("", "ترکیب انتخاب‌شده در حال حاضر فعال نیست.");
                return View(vm);
            }

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var files = new List<PrintFileViewModel>();
            decimal totalPrice = 0;

            foreach (var file in vm.Files)
            {
                if (file == null || file.Length == 0)
                    continue;

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (ext != ".pdf" && ext != ".docx" && ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    ModelState.AddModelError("", $"فرمت فایل '{file.FileName}' پشتیبانی نمی‌شود.");
                    return View(vm);
                }

                var fileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                int pageCount = 1;
                if (ext == ".pdf")
                {
                    pageCount = await GetPdfPageCount(file);
                }

                // بررسی اینکه چاپ پشت و رو است یا خیر
                bool isDoubleSided = string.Equals(vm.PrintSide?.Trim(), "پشت و رو", StringComparison.OrdinalIgnoreCase);
                int effectivePages;

                if (string.Equals(vm.PrintSide?.Trim(), "پشت و رو", StringComparison.OrdinalIgnoreCase))
                {
                    //محاسبه قیمت صفحات فرد
                    effectivePages = (int)Math.Ceiling(pageCount / 2.0); 
                }
                else
                {
                    effectivePages = pageCount;
                }

                decimal filePrice = effectivePages * vm.CopyCount * pricing.PricePerPage;
                totalPrice += filePrice;

                files.Add(new PrintFileViewModel
                {
                    FileName = file.FileName,
                    FilePath = "~/uploads/" + fileName,
                    PageCount = pageCount,
                    FilePrice = filePrice
                });

            }

            var summary = new BWPrintSummaryViewModel
            {
                PaperType = vm.PaperType,
                PaperSize = vm.PaperSize,
                PrintSide = vm.PrintSide,
                CopyCount = vm.CopyCount,
                TotalPages = files.Sum(f => f.PageCount),
                TotalPrice = totalPrice,
                Files = files,
                Description = vm.Description,
                BindingType = vm.BindingType,
            };

            TempData["PrintSummary"] = JsonConvert.SerializeObject(summary);
            return RedirectToAction("BlackWhitePrintStep2");
        }
        
        public IActionResult BlackWhitePrintStep2()
        {
            if (TempData["PrintSummary"] == null)
                return RedirectToAction("BlackWhitePrintForm");

            var json = TempData["PrintSummary"] as string;
            var vm = JsonConvert.DeserializeObject<BWPrintSummaryViewModel>(json);

            TempData["PrintSummary"] = json; // برای مراحل بعدی دوباره ست می‌کنیم
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartFromPrint()
        {
            var json = TempData["PrintSummary"] as string;
            if (json == null)
                return RedirectToAction("BlackWhitePrintForm");

            var vm = JsonConvert.DeserializeObject<BWPrintSummaryViewModel>(json);

            // ساختن شی PrintRequest
            var printRequest = new PrintRequest
            {
                CreatedAt = DateTime.Now,
                Status = "در انتظار تأیید",
                ServiceType = "BlackWhite",
                BlackWhitePrintDetail = new BlackWhitePrintDetail
                {
                    PaperType = vm.PaperType,
                    PaperSize = vm.PaperSize,
                    PrintSide = vm.PrintSide,
                    CopyCount = vm.CopyCount,
                    TotalPages = vm.TotalPages,
                    TotalPrice = vm.TotalPrice,
                    FilesJson = JsonConvert.SerializeObject(vm.Files),
                    Description = vm.Description,
                    BindingType = vm.BindingType,
                }
            };
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // کاربر لاگین نکرده → می‌تونی برگردونی به صفحه لاگین
                return RedirectToAction("Login", "Account");
            }

            printRequest.UserId = userId;

            _context.PrintRequests.Add(printRequest);
            await _context.SaveChangesAsync();

            // افزودن آیدی به سبد خرید داخل Session
            var existing = HttpContext.Session.GetString("CartPrints");
            var list = string.IsNullOrEmpty(existing)
                ? new List<int>()
                : JsonConvert.DeserializeObject<List<int>>(existing);

            list.Add(printRequest.Id);
            HttpContext.Session.SetString("CartPrints", JsonConvert.SerializeObject(list));

            return RedirectToAction("Index", "Cart");
        }


        // GET: نمایش فرم چاپ رنگی
        [HttpGet]
        public IActionResult ColorPrintForm()
        {
            var vm = new ColorPrintRequestViewModel();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ColorPrintForm(ColorPrintRequestViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var pricing = await _context.printPricings.FirstOrDefaultAsync(p =>
                p.PaperType == vm.PaperType &&
                p.PaperSize == vm.PaperSize &&
                p.IsDoubleSided == string.Equals(vm.PrintSide.Trim(), "پشت و رو", StringComparison.OrdinalIgnoreCase) &&
                p.IsAvailable);

            if (pricing == null)
            {
                ModelState.AddModelError("", "ترکیب انتخاب‌شده در حال حاضر فعال نیست.");
                return View(vm);
            }

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var files = new List<PrintFileViewModel>();
            decimal totalPrice = 0;

            foreach (var file in vm.Files)
            {
                if (file == null || file.Length == 0)
                    continue;

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (ext != ".pdf" && ext != ".docx" && ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    ModelState.AddModelError("", $"فرمت فایل '{file.FileName}' پشتیبانی نمی‌شود.");
                    return View(vm);
                }

                var fileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                int pageCount = 1;
                if (ext == ".pdf")
                {
                    pageCount = await GetPdfPageCount(file);
                }

                // بررسی اینکه چاپ پشت و رو است یا خیر
                bool isDoubleSided = string.Equals(vm.PrintSide?.Trim(), "پشت و رو", StringComparison.OrdinalIgnoreCase);
                int effectivePages;

                if (string.Equals(vm.PrintSide?.Trim(), "پشت و رو", StringComparison.OrdinalIgnoreCase))
                {
                    //محاسبه قیمت صفحات فرد
                    effectivePages = (int)Math.Ceiling(pageCount / 2.0);
                }
                else
                {
                    effectivePages = pageCount;
                }

                decimal filePrice = effectivePages * vm.CopyCount * pricing.PricePerPage;
                totalPrice += filePrice;

                files.Add(new PrintFileViewModel
                {
                    FileName = file.FileName,
                    FilePath = "~/uploads/" + fileName,
                    PageCount = pageCount,
                    FilePrice = filePrice
                });

            }

            var summary = new ColorPrintSummaryViewModel
            {
                PaperType = vm.PaperType,
                PaperSize = vm.PaperSize,
                PrintSide = vm.PrintSide,
                CopyCount = vm.CopyCount,
                TotalPages = files.Sum(f => f.PageCount),
                TotalPrice = totalPrice,
                Files = files,
                Description = vm.Description,
                BindingType = vm.BindingType,
            };

            TempData["PrintSummary"] = JsonConvert.SerializeObject(summary);
            return RedirectToAction("ColorPrintStep2");
        }

        
        public IActionResult ColorPrintStep2()
        {
            if (TempData["PrintSummary"] == null)
                return RedirectToAction("ColorPrintForm");

            var json = TempData["PrintSummary"] as string;
            var vm = JsonConvert.DeserializeObject<ColorPrintSummaryViewModel>(json);

            TempData["PrintSummary"] = json; // برای مراحل بعدی دوباره ست می‌کنیم
            return View(vm);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartColorFromPrint()
        {
            var json = TempData["PrintSummary"] as string;
            if (json == null)
                return RedirectToAction("ColorPrintForm");

            var vm = JsonConvert.DeserializeObject<ColorPrintSummaryViewModel>(json);

            // ساختن شی PrintRequest
            var printRequest = new PrintRequest
            {
                CreatedAt = DateTime.Now,
                Status = "در انتظار تأیید",
                ServiceType = "Color",
                ColorPrintDetail = new ColorPrintDetail
                {
                    PaperType = vm.PaperType,
                    PaperSize = vm.PaperSize,
                    PrintSide = vm.PrintSide,
                    CopyCount = vm.CopyCount,
                    TotalPages = vm.TotalPages,
                    TotalPrice = vm.TotalPrice,
                    FilesJson = JsonConvert.SerializeObject(vm.Files),
                    BindingType = vm.BindingType,
                }
            };
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // کاربر لاگین نکرده → می‌تونی برگردونی به صفحه لاگین
                return RedirectToAction("Login", "Account");
            }

            printRequest.UserId = userId;

            _context.PrintRequests.Add(printRequest);
            await _context.SaveChangesAsync();

            // افزودن آیدی به سبد خرید داخل Session
            var existing = HttpContext.Session.GetString("CartPrints");
            var list = string.IsNullOrEmpty(existing)
                ? new List<int>()
                : JsonConvert.DeserializeObject<List<int>>(existing);

            list.Add(printRequest.Id);
            HttpContext.Session.SetString("CartPrints", JsonConvert.SerializeObject(list));

            return RedirectToAction("Index", "Cart");
        }

        // GET: نمایش فرم چاپ پلان
        [HttpGet]
        public IActionResult PlanPrintForm()
        {
            var vm = new PlanPrintRequestViewModel();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlanPrintForm(PlanPrintRequestViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

           

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var files = new List<PrintFileViewModel>();
            
            foreach (var file in vm.Files)
            {
                if (file == null || file.Length == 0)
                    continue;

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".dwg", ".dxf" }; //شامل فرمت اتوکد

                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", $"فرمت فایل '{file.FileName}' پشتیبانی نمی‌شود. پسوندهای مجاز: {string.Join(", ", allowedExtensions)}");
                    return View(vm);
                }



                var fileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

               
              
                files.Add(new PrintFileViewModel
                {
                    FileName = file.FileName,
                    FilePath = "~/uploads/" + fileName,
                    PageCount = 0, 
                    FilePrice = 0  // اگر قیمت رو هم نمی‌خوای محاسبه کنی
                });

            }

            var summary = new PlanPrintSummaryViewModel
            {
                PaperType = vm.PaperType,
                CopyCount = vm.CopyCount,
                Files = files,
                SizeOrScaleDescription = vm.SizeOrScaleDescription,
                AdditionalDescription = vm.AdditionalDescription,
                BindingType = vm.BindingType,
                printType = vm.printType,
            };

            TempData["PrintSummary"] = JsonConvert.SerializeObject(summary);
            return RedirectToAction("PlanPrintStep2");
        }

        // مرحله دوم: نمایش خلاصه اطلاعات قبل از افزودن به سبد
        public IActionResult PlanPrintStep2()
        {
            if (TempData["PrintSummary"] == null)
                return RedirectToAction("PlanPrintForm");

            var json = TempData["PrintSummary"] as string;
            var vm = JsonConvert.DeserializeObject<PlanPrintSummaryViewModel>(json);

            TempData["PrintSummary"] = json; // برای مراحل بعدی دوباره ست می‌کنیم
            return View(vm);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartPlanFromPrint()
        {
            var json = TempData["PrintSummary"] as string;
            if (json == null)
                return RedirectToAction("PlanPrintForm");

            var vm = JsonConvert.DeserializeObject<PlanPrintSummaryViewModel>(json);

            // ساختن شی PrintRequest
            var printRequest = new PrintRequest
            {
                CreatedAt = DateTime.Now,
                Status = "در انتظار تأیید",
                ServiceType = "Plan",
                PlanPrintDetail = new PlanPrintDetail
                {
                    PaperType = vm.PaperType,
                    CopyCount = vm.CopyCount,
                    FilesJson = JsonConvert.SerializeObject(vm.Files),
                    SizeOrScaleDescription = vm.SizeOrScaleDescription,
                    AdditionalDescription = vm.AdditionalDescription,
                    printType = vm.printType,
                    BindingType = vm.BindingType,

                }
            };
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // کاربر لاگین نکرده → می‌تونی برگردونی به صفحه لاگین
                return RedirectToAction("Login", "Account");
            }

            printRequest.UserId = userId;

            _context.PrintRequests.Add(printRequest);
            await _context.SaveChangesAsync();

            // افزودن آیدی به سبد خرید داخل Session
            var existing = HttpContext.Session.GetString("CartPrints");
            var list = string.IsNullOrEmpty(existing)
                ? new List<int>()
                : JsonConvert.DeserializeObject<List<int>>(existing);

            list.Add(printRequest.Id);
            HttpContext.Session.SetString("CartPrints", JsonConvert.SerializeObject(list));

            return RedirectToAction("Index", "Cart");
        }

        private async Task<int> GetPdfPageCount(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var pdf = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);
            return pdf.PageCount;
        }





    }
}
