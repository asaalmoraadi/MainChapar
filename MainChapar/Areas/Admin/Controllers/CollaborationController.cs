using MainChapar.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

namespace MainChapar.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class CollaborationController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public CollaborationController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Admin/Collaborations
        // لیست رزومه‌ها
        public async Task<IActionResult> Index()
        {
            var collaborations = await _context.collaborations
                .Include(c => c.UploadedFiles)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View(collaborations);
        }

        

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DownloadAllResumes(int collaborationId)
        {
            var collaboration = await _context.collaborations
                .Include(c => c.UploadedFiles)
                .FirstOrDefaultAsync(c => c.Id == collaborationId);

            if (collaboration == null || !collaboration.UploadedFiles.Any())
                return NotFound();

            //  اگر فقط یک فایل باشه مستقیم دانلود کن
            if (collaboration.UploadedFiles.Count == 1)
            {
                var singleFile = collaboration.UploadedFiles.First();
                var path = Path.Combine(_env.WebRootPath, singleFile.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(path))
                {
                    return PhysicalFile(path, "application/octet-stream", Path.GetFileName(path));
                }
            }

            //  اگر بیشتر از یک فایل بود زیپ کن
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in collaboration.UploadedFiles)
                    {
                        var path = Path.Combine(_env.WebRootPath, file.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(path))
                        {
                            var entry = archive.CreateEntry(Path.GetFileName(path), CompressionLevel.Fastest);
                            using var entryStream = entry.Open();
                            using var fileStream = System.IO.File.OpenRead(path);
                            await fileStream.CopyToAsync(entryStream);
                        }
                    }
                }

                return File(memoryStream.ToArray(), "application/zip", $"{collaboration.FirstName}_{collaboration.LastName}_Resumes.zip");
            }
        }
    }
}
