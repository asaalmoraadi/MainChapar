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
            //حذف فاصله اضافی پشت و رو و تک رو
            var normalizedPrintSide = vm.PrintSide?.Trim().ToLower();

            var pricing = await _context.printPricings.FirstOrDefaultAsync(p =>
                p.PaperType == vm.PaperType &&
                p.PaperSize == vm.PaperSize &&
                p.PrintSide.Trim().ToLower() == normalizedPrintSide &&
                p.IsAvailable);

            if (pricing == null)
            {
                ModelState.AddModelError("", "ترکیب انتخاب‌شده در حال حاضر فعال نیست.");
                return View(vm);
            }

            //مسیر ذخیره سازی در پوشه uploads
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            //اگر پوشه نبود آن را بساز
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var files = new List<PrintFileViewModel>();
            decimal totalPrice = 0;

            foreach (var file in vm.Files)
            {
                if (file == null || file.Length == 0)
                    continue;

                //بررسی پسوند
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (ext != ".pdf" && ext != ".docx" && ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    ModelState.AddModelError("", $"فرمت فایل '{file.FileName}' پشتیبانی نمی‌شود.");
                    return View(vm);
                }

                //ایجاد فایل و کپی آن روی سرور
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
                    FilePath = "uploads/" + fileName,
                    PageCount = pageCount,
                    FilePrice = filePrice
                });

            }
            if (files.Count == 0 || totalPrice == 0)
            {
                ModelState.AddModelError("", "هیچ فایل معتبری برای چاپ بارگذاری نشده یا قیمت نهایی صفر است.");
                return View(vm);
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

            HttpContext.Session.SetString("PrintSummary", JsonConvert.SerializeObject(summary));
            //
            return RedirectToAction("BlackWhitePrintStep2");
        }
        public IActionResult BlackWhitePrintStep2()
        {

            //دریافت اطلاعات فرم
            var json = HttpContext.Session.GetString("PrintSummary");
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("BlackWhitePrintForm");
            //تبدیل از رشته json به vm
            var vm = JsonConvert.DeserializeObject<BWPrintSummaryViewModel>(json);
            
            return View(vm);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]  
        public async Task<IActionResult> AddToCartFromPrint()
        {
            var json = HttpContext.Session.GetString("PrintSummary");
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("BlackWhitePrintForm");

            var vm = JsonConvert.DeserializeObject<BWPrintSummaryViewModel>(json);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "User");

            if (vm.TotalPrice == 0 || vm.TotalPrice == null)
            {
                ModelState.AddModelError("", "قیمت نهایی سفارش مشخص نشده است.");
                return RedirectToAction("BlackWhitePrintForm");
            }

            var printRequest = new PrintRequest
            {
                CreatedAt = DateTime.Now,
                Status = "Draft",
                IsFinalized = false,
                ServiceType = "BlackWhite",
                UserId = userId,
                TotalPrice = vm.TotalPrice,
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

            _context.PrintRequests.Add(printRequest);
            await _context.SaveChangesAsync();

            //   بخش ذخیره فایل‌ها به دیتابیس
            if (vm.Files != null && vm.Files.Any())
            {
                foreach (var f in vm.Files)
                {
                    var printFile = new PrintFile
                    {
                        PrintRequestId = printRequest.Id, // لینک به سفارش
                        FileName = f.FileName,
                        FilePath = f.FilePath,            // مسیر درست ذخیره شده
                        PageCount = f.PageCount,
                    };
                    _context.PrintFiles.Add(printFile); // Add تک‌تک فایل‌ها
                }

                await _context.SaveChangesAsync(); // این SaveChangesAsync مهمه که فایل‌ها واقعاً ذخیره بشن
            }

            // بررسی دوباره ذخیره شدن سفارش و قیمت‌ها
            var savedRequest = await _context.PrintRequests
                .Include(p => p.BlackWhitePrintDetail)
                .FirstOrDefaultAsync(p => p.Id == printRequest.Id);

            if (savedRequest == null || savedRequest.TotalPrice == 0 || savedRequest.BlackWhitePrintDetail.TotalPrice == 0)
            {
                TempData["CartDebug"] = "خطا: قیمت در دیتابیس ذخیره نشده است.";
                return RedirectToAction("BlackWhitePrintForm");
            }

            var cartItem = new CartPrintItems
            {
                PrintRequestId = printRequest.Id,
                UserId = userId
            };

            _context.CartPrintItems.Add(cartItem);
            await _context.SaveChangesAsync();

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

            var normalizedPrintSide = vm.PrintSide?.Trim().ToLower();

            var pricing = await _context.printPricings.FirstOrDefaultAsync(p =>
                p.PaperType == vm.PaperType &&
                p.PaperSize == vm.PaperSize &&
                p.PrintSide.Trim().ToLower() == normalizedPrintSide &&
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

                bool isDoubleSided = string.Equals(vm.PrintSide?.Trim(), "پشت و رو", StringComparison.OrdinalIgnoreCase);
                int effectivePages = isDoubleSided ? (int)Math.Ceiling(pageCount / 2.0) : pageCount;

                decimal filePrice = effectivePages * vm.CopyCount * pricing.PricePerPage;
                totalPrice += filePrice;

                files.Add(new PrintFileViewModel
                {
                    FileName = file.FileName,
                    FilePath = "uploads/" + fileName,
                    PageCount = pageCount,
                    FilePrice = filePrice
                });
            }

            if (files.Count == 0 || totalPrice == 0)
            {
                ModelState.AddModelError("", "هیچ فایل معتبری برای چاپ بارگذاری نشده یا قیمت نهایی صفر است.");
                return View(vm);
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

            HttpContext.Session.SetString("PrintSummary_Color", JsonConvert.SerializeObject(summary)); // ← به جای TempData
            return RedirectToAction("ColorPrintStep2");
        }


        public IActionResult ColorPrintStep2()
        {
            var json = HttpContext.Session.GetString("PrintSummary_Color");
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("ColorPrintForm");

            var vm = JsonConvert.DeserializeObject<ColorPrintSummaryViewModel>(json);
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartColorFromPrint()
        {
            var json = HttpContext.Session.GetString("PrintSummary_Color");
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("ColorPrintForm");

            var vm = JsonConvert.DeserializeObject<ColorPrintSummaryViewModel>(json);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "User");

            if (vm.TotalPrice == 0 || vm.TotalPrice == null)
            {
                ModelState.AddModelError("", "قیمت نهایی سفارش مشخص نشده است.");
                return RedirectToAction("ColorPrintForm");
            }

            var printRequest = new PrintRequest
            {
                CreatedAt = DateTime.Now,
                Status = "Draft",       // جلوگیری از Completed شدن زودهنگام
                IsFinalized = false,    // سفارش هنوز نهایی نیست
                ServiceType = "Color",
                UserId = userId,
                TotalPrice = vm.TotalPrice,
                ColorPrintDetail = new ColorPrintDetail
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

            _context.PrintRequests.Add(printRequest);
            await _context.SaveChangesAsync();

            //  ذخیره فایل‌ها در جدول PrintFiles
            if (vm.Files != null && vm.Files.Any())
            {
                foreach (var f in vm.Files)
                {
                    var printFile = new PrintFile
                    {
                        PrintRequestId = printRequest.Id,
                        FileName = f.FileName,
                        FilePath = f.FilePath,
                        PageCount = f.PageCount,
                    };
                    _context.PrintFiles.Add(printFile);
                }

                await _context.SaveChangesAsync();
            }

            //  بررسی دوباره ذخیره شدن سفارش و قیمت‌ها
            var savedRequest = await _context.PrintRequests
                .Include(p => p.ColorPrintDetail)
                .FirstOrDefaultAsync(p => p.Id == printRequest.Id);

            if (savedRequest == null || savedRequest.TotalPrice == 0 || savedRequest.ColorPrintDetail.TotalPrice == 0)
            {
                TempData["CartDebug"] = "خطا: قیمت در دیتابیس ذخیره نشده است.";
                return RedirectToAction("ColorPrintForm");
            }

            // افزودن سفارش به سبد خرید
            var cartItem = new CartPrintItems
            {
                PrintRequestId = printRequest.Id,
                UserId = userId
            };

            _context.CartPrintItems.Add(cartItem);
            await _context.SaveChangesAsync();

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
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".dwg", ".dxf" };

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
                    FilePath = "uploads/" + fileName,
                    PageCount = 0,
                    FilePrice = 0
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

            HttpContext.Session.SetString("PrintSummary_Plan", JsonConvert.SerializeObject(summary));
            return RedirectToAction("PlanPrintStep2");
        }

        public IActionResult PlanPrintStep2()
        {
            var json = HttpContext.Session.GetString("PrintSummary_Plan");
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("PlanPrintForm");

            var vm = JsonConvert.DeserializeObject<PlanPrintSummaryViewModel>(json);
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartPlanFromPrint()
        {
            var json = HttpContext.Session.GetString("PrintSummary_Plan");
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("PlanPrintForm");

            var vm = JsonConvert.DeserializeObject<PlanPrintSummaryViewModel>(json);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "User");

            var printRequest = new PrintRequest
            {
                CreatedAt = DateTime.Now,
                Status = "Draft",        
                IsFinalized = false,     
                ServiceType = "Plan",
                TotalPrice = 0,         
                UserId = userId,
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

            _context.PrintRequests.Add(printRequest);
            await _context.SaveChangesAsync();

            //  ذخیره فایل‌ها در جدول PrintFiles
            if (vm.Files != null && vm.Files.Any())
            {
                foreach (var f in vm.Files)
                {
                    var printFile = new PrintFile
                    {
                        PrintRequestId = printRequest.Id,
                        FileName = f.FileName,
                        FilePath = f.FilePath,
                        PageCount = f.PageCount
                    };
                    _context.PrintFiles.Add(printFile);
                }
                await _context.SaveChangesAsync();
            }

            var cartPrintItem = new CartPrintItems
            {
                PrintRequestId = printRequest.Id,
                UserId = userId
            };
            _context.CartPrintItems.Add(cartPrintItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Cart");
        }

        // GET: نمایش فرم چاپ لمینیت
        [HttpGet]
        public IActionResult LaminatePrintForm()
        {
            var vm = new LaminatePrintRequestViewModel();
            return View(vm);
        }
        public async Task<IActionResult> LaminatePrintForm(LaminatePrintRequestViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var normalizedPrintSide = vm.PrintSide?.Trim().ToLower();

            var pricing = await _context.printPricings.FirstOrDefaultAsync(p =>
                p.PaperType == vm.PaperType &&
                p.PaperSize == vm.PaperSize &&
                p.PrintSide.Trim().ToLower() == normalizedPrintSide &&
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

                decimal filePrice = pageCount * vm.CopyCount * pricing.PricePerPage;
                totalPrice += filePrice;

                files.Add(new PrintFileViewModel
                {
                    FileName = file.FileName,
                    FilePath = "uploads/" + fileName,
                    PageCount = pageCount,
                    FilePrice = filePrice
                });
            }

            if (files.Count == 0 || totalPrice == 0)
            {
                ModelState.AddModelError("", "هیچ فایل معتبری برای چاپ بارگذاری نشده یا قیمت نهایی صفر است.");
                return View(vm);
            }

            var summary = new LaminatePrintSummaryViewModel
            {
                PaperType = vm.PaperType,
                PaperSize = vm.PaperSize,
                PrintSide = vm.PrintSide,
                CopyCount = vm.CopyCount,
                TotalPages = files.Sum(f => f.PageCount),
                TotalPrice = totalPrice,
                Files = files,
                Description = vm.Description,
                printType = vm.printType,
                LaminateType = vm.LaminateType,
                CornerType = vm.CornerType,
            };

            
            HttpContext.Session.SetString("PrintSummary_Laminate", JsonConvert.SerializeObject(summary));
            return RedirectToAction("LaminatePrintStep2");
        }

        public IActionResult LaminatePrintStep2()
        {
            var json = HttpContext.Session.GetString("PrintSummary_Laminate");
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("LaminatePrintForm");

            var vm = JsonConvert.DeserializeObject<LaminatePrintSummaryViewModel>(json); 
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartLaminateFromPrint()
        {
            var json = HttpContext.Session.GetString("PrintSummary_Laminate");
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("LaminatePrintForm");

            var vm = JsonConvert.DeserializeObject<LaminatePrintSummaryViewModel>(json);

            if (vm.TotalPrice == 0 || vm.TotalPrice == null)
            {
                ModelState.AddModelError("", "قیمت نهایی سفارش مشخص نشده است.");
                return RedirectToAction("LaminatePrintForm");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "User");

            var printRequest = new PrintRequest
            {
                CreatedAt = DateTime.Now,
                Status = "Draft",
                IsFinalized = false,
                ServiceType = "Laminate",
                TotalPrice = vm.TotalPrice,
                UserId = userId,
                LaminateDetail = new LaminateDetail
                {
                    PaperType = vm.PaperType,
                    PaperSize = vm.PaperSize,
                    PrintSide = vm.PrintSide,
                    CopyCount = vm.CopyCount,
                    TotalPages = vm.TotalPages,
                    TotalPrice = vm.TotalPrice,
                    FilesJson = JsonConvert.SerializeObject(vm.Files),
                    printType = vm.printType,
                    LaminateType = vm.LaminateType,
                    CornerType = vm.CornerType,
                    Description = vm.Description,
                }
            };

            _context.PrintRequests.Add(printRequest);
            await _context.SaveChangesAsync();

            // ✅ ذخیره فایل‌ها در جدول PrintFiles
            if (vm.Files != null && vm.Files.Any())
            {
                foreach (var f in vm.Files)
                {
                    var printFile = new PrintFile
                    {
                        PrintRequestId = printRequest.Id,
                        FileName = f.FileName,
                        FilePath = f.FilePath,
                        PageCount = f.PageCount,
                    };
                    _context.PrintFiles.Add(printFile);
                }
                await _context.SaveChangesAsync();
            }

            //  بررسی دوباره ذخیره شدن سفارش و قیمت
            var savedRequest = await _context.PrintRequests
                .Include(p => p.LaminateDetail)
                .FirstOrDefaultAsync(p => p.Id == printRequest.Id);

            if (savedRequest == null || savedRequest.TotalPrice == 0 || savedRequest.LaminateDetail.TotalPrice == 0)
            {
                TempData["CartDebug"] = "خطا: قیمت در دیتابیس ذخیره نشده است.";
                return RedirectToAction("LaminatePrintForm");
            }

            var cartPrintItem = new CartPrintItems
            {
                PrintRequestId = printRequest.Id,
                UserId = userId
            };
            _context.CartPrintItems.Add(cartPrintItem);
            await _context.SaveChangesAsync();

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
