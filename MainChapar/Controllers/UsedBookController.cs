using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MainChapar.Data;
using MainChapar.Models;
using MainChapar.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace MainChapar.Controllers
{
    public class UsedBookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UsedBookController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UsedBook
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var myBooks = await _context.usedBooks
                .Where(b => b.UserId == userId)
                .Include(b => b.Images)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(myBooks);
        }

        // GET: UsedBook/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usedBook = await _context.usedBooks
                .Include(u => u.User)
                .Include(u => u.Images) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (usedBook == null)
            {
                return NotFound();
            }

            return View(usedBook);
        }

        // GET: UsedBook/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.users, "Id", "Id");
            return View();
        }

        // POST: UsedBook/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsedBookCreateViewModel model)
        {

            if (!ModelState.IsValid)
                return View(model);
            var userId = _userManager.GetUserId(User); // آی‌دی کاربر لاگین‌شده

            var usedBook = new UsedBook
            {
                Title = model.Title,
                Author = model.Author,
                Description = model.Description,
                Price = model.Price,
                ContactNumber = model.ContactNumber,
                CreatedAt = DateTime.Now,
                IsApproved = false, // منتظر تایید ادمین
                UserId = userId,
                Images = new List<UsedBookImage>()
            };

            // ذخیره تصاویر
            foreach (var file in model.Images)
            {
                if (file.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/books", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    usedBook.Images.Add(new UsedBookImage
                    {
                        ImagePath = "/uploads/books/" + fileName
                    });
                }
            }

            _context.usedBooks.Add(usedBook);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index"); // یا هر صفحه‌ای که میخوای بعد از ثبت نمایش بدی
        }

        // GET: UsedBook/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usedBook = await _context.usedBooks.FindAsync(id);
            if (usedBook == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.users, "Id", "Id", usedBook.UserId);
            return View(usedBook);
        }

        // POST: UsedBook/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,Description,Price,ContactNumber,CreatedAt,IsApproved,UserId")] UsedBook usedBook)
        {
            if (id != usedBook.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usedBook);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsedBookExists(usedBook.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.users, "Id", "Id", usedBook.UserId);
            return View(usedBook);
        }

        // GET: UsedBook/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usedBook = await _context.usedBooks
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usedBook == null)
            {
                return NotFound();
            }

            return View(usedBook);
        }

        // POST: UsedBook/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usedBook = await _context.usedBooks.FindAsync(id);
            if (usedBook != null)
            {
                _context.UsedBook.Remove(usedBook);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsedBookExists(int id)
        {
            return _context.usedBooks.Any(e => e.Id == id);
        }
    }
}
