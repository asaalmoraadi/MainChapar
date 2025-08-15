using MainChapar.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainChapar.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class CollaborationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CollaborationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Collaborations
        public async Task<IActionResult> Index()
        {
            var collaborations = await _context.collaborations
                .Include(c => c.UploadedFiles)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View(collaborations);
        }
    }
}
