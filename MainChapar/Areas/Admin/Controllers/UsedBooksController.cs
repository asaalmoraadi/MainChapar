using MainChapar.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainChapar.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class UsedBooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsedBooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminUsedBook
        public async Task<IActionResult> Index()
        {
            var books = await _context.usedBooks
                .Include(b => b.User)
                .Include(b => b.Images)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(books);
        }

        // GET: AdminUsedBook/Approve/5
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.usedBooks.FindAsync(id);
            if (book == null) return NotFound();

            book.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: AdminUsedBook/Reject/5
        public async Task<IActionResult> Reject(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.usedBooks.FindAsync(id);
            if (book == null) return NotFound();

            book.IsApproved = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
