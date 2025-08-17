using MainChapar.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MainChapar.Models;
using MainChapar.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace MainChapar.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class PrintRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public PrintRequestsController(ApplicationDbContext context)
        {
            _context = context;  
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
            var request = await _context.PrintRequests
              .Include(r => r.User)
              .Include(r => r.ColorPrintDetail)
              .Include(r => r.BlackWhitePrintDetail)
              .Include(r => r.PlanPrintDetail)
              .Include(r => r.Files)
              .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            // شرط جدید: اگر نهایی نشده، اجازه مشاهده نده
            //if (!request.IsFinalized)
            //{
            //    TempData["Error"] = "این سفارش هنوز نهایی نشده و قابل بررسی نیست.";
            //    return RedirectToAction("Index");
            //}

            // اینجا می‌توانیم تعداد فایل‌ها را چک کنیم
            var files = await _context.PrintFiles
                .Where(f => f.PrintRequestId == id)
                .ToListAsync();

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

        // POST: Admin/PrintRequests/SetStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, string status)
        {
            var request = await _context.PrintRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = status;
            await _context.SaveChangesAsync();
            // آپدیت وضعیت PickupRequest مرتبط
            // پیدا کردن PickupRequest های مرتبط با این PrintRequest
            var pickupRequestIds = await _context.pickupPrintItems
                .Where(pi => pi.PrintRequestId == id)
                .Select(pi => pi.PickupRequestId)
                .Distinct()
                .ToListAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            var file = await _context.PrintFiles.FindAsync(fileId);
            if (file == null)
                return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath);

            if (!System.IO.File.Exists(filePath))
                return NotFound($"فایل در سرور یافت نشد: {filePath}");

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var contentType = GetContentType(filePath);
            return File(memory, contentType, file.FileName); // اینجا فقط اسم دانلودی
        }

        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
    {
        {".pdf", "application/pdf"},
        {".doc", "application/msword"},
        {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
        {".png", "image/png"},
        {".jpg", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".bmp", "image/bmp"}
    };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }
    }


}
