using MainChapar.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MainChapar.Models;

namespace MainChapar.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            var requests = await _context.PrintRequests.Include(p => p.User).ToListAsync();
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

            return RedirectToAction(nameof(Index));
        }
    }
}
