using MainChapar.Data;
using MainChapar.Models;
using MainChapar.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MainChapar.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ServicesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Services/Typing
        public IActionResult Typing()
        {
            return View();
        }

        // POST: /Services/Typing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Typing(TypingRequestViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            //فایل اپلود شده کاربر را ذخیره و مسیر را برمیگرداند
            var filePath = await SaveUploadedFileAsync(model.UploadedFile);

            var request = new ServiceRequest
            {
                Name = model.Name,
                Family = model.Family,
                PhoneNumber = model.PhoneNumber,
                Deadline = model.Deadline,
                UploadedFilePath = filePath,
                Description = model.Description,
                Type = ServiceType.Typing
            };

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success");
        }

        // GET: /Services/PowerPoint
        public IActionResult PowerPoint()
        {
            return View();
        }

        // POST: /Services/PowerPoint
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PowerPoint(PowerPointRequestViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var filePath = await SaveUploadedFileAsync(model.UploadedFile);

            var request = new ServiceRequest
            {
                Name = model.Name,
                Family = model.Family,
                PhoneNumber = model.PhoneNumber,
                Deadline = model.Deadline,
                UploadedFilePath = filePath,
                Description = model.Description,
                ProjectTitle = model.ProjectTitle,
                Type = ServiceType.PowerPoint
            };

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success");
        }

        // GET: /Services/Success
        public IActionResult Success()
        {
            return View();
        }

        // تابع کمکی برای ذخیره فایل
        private async Task<string?> SaveUploadedFileAsync(IFormFile? file)
        {
            if (file == null) return null;

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var uploadsFolder = Path.Combine(_env.WebRootPath, "assets", "uploads");

            // اطمینان از وجود پوشه
            if (!Directory.Exists(uploadsFolder))
            {
                //ایجاد پوشه
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/assets/uploads/" + fileName;
        }



    }
}
