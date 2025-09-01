using MainChapar.Data;
using MainChapar.Models;
using Microsoft.AspNetCore.Mvc;

namespace MainChapar.Controllers
{
    public class CollaborationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CollaborationController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

       
        [HttpPost]
        public async Task<IActionResult> Create(Collaboration collaboration, List<IFormFile> resume)
        {

            ModelState.Remove("UploadedFiles");
            if (resume == null || resume.Count == 0)
            {
                ModelState.AddModelError("resume", "حداقل یک فایل رزومه باید انتخاب شود.");
            }

            if (!ModelState.IsValid)
            {
                return View(collaboration);
            }

            if (resume != null && resume.Count > 0)
            {
                collaboration.UploadedFiles = new List<UploadedFile>();
                var uploads = Path.Combine(_env.WebRootPath, "uploads", "collaborators");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                foreach (var file in resume)
                {
                    if (file.Length > 0)
                    {
                        // اسم فایل یکتا تولید می‌کنیم
                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(uploads, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        // فایل ذخیره شد، حالا شی UploadedFile می‌سازیم

                        collaboration.UploadedFiles.Add(new UploadedFile
                        {
                            FilePath = "/uploads/collaborators/" + uniqueFileName
                        });
                    }
                }
            }

            _context.collaborations.Add(collaboration);
            await _context.SaveChangesAsync();

            return RedirectToAction("index","Home"); // یا صفحه تایید دلخواه
        }


        public IActionResult ThankYou()
        {
            return View();
        }
    }

}

