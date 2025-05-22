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

        private Task FillDropDownLists(BlackWhitePrintRequestViewModel vm)
        {
            vm.PaperTypes = new List<string> { "گلاسه", "ساده", "گلاسه سنگین" };
            vm.PaperSizes = new List<string> { "A4", "A3", "A5" };
            vm.PrintSides = new List<string> { "تک رو", "پشت و رو" };
            return Task.CompletedTask;
        }

        // نمایش فرم درخواست چاپ سیاه و سفید
        [HttpGet]
        public IActionResult BlackWhitePrintForm()
        {
            var model = new BlackWhitePrintRequestViewModel
            {
                PaperTypes = new List<string> { "گلاسه", "ساده", "گلاسه سنگین" },  // نمونه لیست‌ها
                PaperSizes = new List<string> { "A4", "A3", "A5" },
                PrintSides = new List<string> { "تک رو", "پشت و رو " }
            };

            return View(model);
        }

        // ارسال فرم درخواست چاپ سیاه و سفید
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlackWhitePrintForm(BlackWhitePrintRequestViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await FillDropDownLists(vm); // پر کردن لیست‌ها
                return View("BlackWhitePrintForm", vm);
            }

            // بررسی وجود ترکیب انتخاب‌شده
            var pricing = await _context.printPricings.FirstOrDefaultAsync(p =>
                p.PaperType == vm.PaperType &&
                p.PaperSize == vm.PaperSize &&
                p.IsDoubleSided == (vm.PrintSide == "پشت و رو") &&
                p.IsAvailable);

            if (pricing == null)
            {
                ModelState.AddModelError("", "ترکیب انتخاب‌شده در حال حاضر فعال نیست.");
                await FillDropDownLists(vm); // دوباره پر کن
                return View("BlackWhitePrintForm", vm);
            }

            var printRequest = new PrintRequest
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                ServiceType = "BlackWhite",
                Status = "Submitted",
                CreatedAt = DateTime.Now
            };

            var detail = new BlackWhitePrintDetail
            {
                PaperType = vm.PaperType,
                PaperSize = vm.PaperSize,
                PrintSide = vm.PrintSide,
                ContentType = "Text", // اگر خواستی ContentType رو هم به ViewModel اضافه کن
                CopyCount = vm.CopyCount
            };

            var files = new List<PrintFile>();
            decimal totalPrice = 0;

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var file in vm.Files)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                int pageCount = await GetPdfPageCount(file);
                var printFile = new PrintFile
                {
                    FileName = file.FileName,
                    FilePath = "~/uploads/" + fileName,
                    PageCount = pageCount
                };

                decimal filePrice = pageCount * vm.CopyCount * pricing.PricePerPage;
                totalPrice += filePrice;

                files.Add(printFile);
            }

            detail.TotalPrice = totalPrice;
            printRequest.TotalPrice = totalPrice;
            printRequest.BlackWhitePrintDetail = detail;
            printRequest.Files = files;

            _context.PrintRequests.Add(printRequest);
            await _context.SaveChangesAsync();
            var summary = new BWPrintSummaryViewModel
            {
                PaperType = vm.PaperType,
                PaperSize = vm.PaperSize,
                PrintSide = vm.PrintSide,
                CopyCount = vm.CopyCount,
                TotalPages = files.Sum(f => f.PageCount),
                TotalPrice = totalPrice
            };

            TempData["PrintSummary"] = JsonConvert.SerializeObject(summary);

            return RedirectToAction("BlackWhitePrintStep2");
        }


        public IActionResult BlackWhitePrintStep2()
        {
            if (TempData["PrintSummary"] == null)
                return RedirectToAction("BlackWhitePrintForm"); // اگر داده‌ای نبود، برگرد صفحه اول

            var json = TempData["PrintSummary"] as string;
            var vm = JsonConvert.DeserializeObject<BWPrintSummaryViewModel>(json);

            return View(vm);
        }





        // نمایش فرم درخواست چاپ رنگی
        [HttpGet]
        public IActionResult ColorPrint()
        {
            var model = new ColorPrintRequestViewModel
            {
                
                PaperTypes = new List<string> { "گلاسه", "ساده", "گلاسه سنگین" },  // نمونه لیست‌ها
                PaperSizes = new List<string> { "A4", "A3", "A5" },
                PrintSides = new List<string> { "تک رو", "پشت و رو " }
            };

            return View(model);
        }
        // ارسال فرم درخواست چاپ رنگی
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ColorPrint(ColorPrintRequestDto dto)
        {
            if (!ModelState.IsValid)
                return View("ColorPrint", dto); // در صورت ارور، فرم را دوباره نمایش بده

            // بررسی اعتبار انتخاب‌های کاربر از نظر موجود بودن و فعال بودن
            var pricing = await _context.printPricings
                .FirstOrDefaultAsync(p =>
                    p.PaperType == dto.PaperType &&
                    p.PaperSize == dto.PaperSize &&
                    p.IsDoubleSided == (dto.PrintSide == "پشت و رو") &&  // بر اساس انتخاب یک رو یا دو رو بودن
                    p.IsAvailable);

            if (pricing == null)
            {
                ModelState.AddModelError("", "ترکیب انتخاب‌شده در حال حاضر فعال نیست.");
                return View("ColorPrint", dto);
            }

            // ساخت شیء اصلی سفارش
            var printRequest = new PrintRequest
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                ServiceType = "Color",  // نوع چاپ رنگی
                Status = "Submitted",
                CreatedAt = DateTime.Now
            };

            var detail = new ColorPrintDetail
            {
                PaperType = dto.PaperType,
                PaperSize = dto.PaperSize,
                PrintSide = dto.PrintSide,
                CopyCount = dto.CopyCount
            };

            var files = new List<PrintFile>();
            decimal totalPrice = 0;

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var file in dto.Files)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                int pageCount = await GetPdfPageCount(file);
                var printFile = new PrintFile
                {
                    FileName = file.FileName,
                    FilePath = "~/uploads/" + fileName,
                    PageCount = pageCount
                };

                decimal filePrice = pageCount * dto.CopyCount * pricing.PricePerPage;
                totalPrice += filePrice;

                files.Add(printFile);
            }

            detail.TotalPrice = totalPrice;
            printRequest.TotalPrice = totalPrice;
            printRequest.ColorPrintDetail = detail;
            printRequest.Files = files;

            _context.PrintRequests.Add(printRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserPrintRequests");
        }

        private async Task<int> GetPdfPageCount(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var pdf = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);
            return pdf.PageCount;
        }





    }
}
