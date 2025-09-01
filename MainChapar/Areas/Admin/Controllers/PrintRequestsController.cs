using MainChapar.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MainChapar.Models;
using MainChapar.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.IO.Compression;

namespace MainChapar.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    
    public class PrintRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;
        public PrintRequestsController(ApplicationDbContext context, IWebHostEnvironment env, UserManager<User> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        // GET: Admin/PrintRequests
        public async Task<IActionResult> Index()
        {
            
            var totalPrintServices = _context.PrintRequests.Count();
            var completedPrintServices = _context.PrintRequests.Count(ps => ps.Status == "Completed");
            var RejectedPrintServices = _context.PrintRequests.Count(ps => ps.Status == "Rejected");


            // ارسال به ویو با ViewBag
            ViewBag.totalPrintServices = totalPrintServices;
            ViewBag.completedPrintServices = completedPrintServices;
            ViewBag.RejectedServices = RejectedPrintServices;
            //var requests = await _context.PrintRequests.Include(p => p.User).ToListAsync();
            //بخش جدید
            var requests = await _context.PrintRequests
                .Where(p => p.IsFinalized) //  فقط سفارش‌های نهایی
                .Include(p => p.User)
                .ToListAsync();
            return View(requests);
        }

        // GET: Admin/PrintRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var request = await _context.PrintRequests
                .Include(r => r.User)
                .Include(r => r.ColorPrintDetail)
                .Include(r => r.BlackWhitePrintDetail)
                .Include(r => r.PlanPrintDetail)
                .Include(r => r.LaminateDetail)
                .Include(r => r.PickupPrintItems)
                .Include(r => r.Files)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            
            var files = request.Files;

            Console.WriteLine($"تعداد فایل‌های مرتبط: {files.Count}");

            return View(request);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var request = await _context.PrintRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            request.Status = status;
            await _context.SaveChangesAsync();
            // آپدیت وضعیت PickupRequest مرتبط
            // پیدا کردن PickupRequest های مرتبط با این PrintRequest
            var pickupRequestIds = await _context.pickupPrintItems
                .Where(pi => pi.PrintRequestId == id)
                .Select(pi => pi.PickupRequestId)
                .Distinct()
                .ToListAsync();

            return RedirectToAction(nameof(Details), new { id });
        }


        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DownloadPrintFiles(int id)
        {
            var printRequest = await _context.PrintRequests
                .Include(p => p.Files)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (printRequest == null || printRequest.Files == null || !printRequest.Files.Any())
                return NotFound("هیچ فایلی برای این سفارش یافت نشد.");

            // اگه فقط یک فایل باشه، مستقیم همون فایل رو برگردون
            if (printRequest.Files.Count == 1)
            {
                var singleFile = printRequest.Files.First();
                var filePath = Path.Combine(_env.WebRootPath, singleFile.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (!System.IO.File.Exists(filePath))
                    return NotFound("فایل مورد نظر یافت نشد.");

                // Generic binary content type (forces browser to download the file if type is unknown)
                var contentType = "application/octet-stream";
                return PhysicalFile(filePath, contentType, singleFile.FileName);
            }

            // اگه چند فایل باشه -> زیپ کن
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in printRequest.Files)
                    {
                        var filePath = Path.Combine(_env.WebRootPath, file.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                        if (System.IO.File.Exists(filePath))
                        {
                            var entry = zipArchive.CreateEntry(file.FileName, CompressionLevel.Fastest);
                            using (var entryStream = entry.Open())
                            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            {
                                await fileStream.CopyToAsync(entryStream);
                            }
                        }
                    }
                }

                return File(memoryStream.ToArray(), "application/zip", $"PrintRequest_{id}_Files.zip");
            }
        }

        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
        {
            {".pdf", "application/pdf"},
            {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".png", "image/png"}
        };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }
    }

    


}
