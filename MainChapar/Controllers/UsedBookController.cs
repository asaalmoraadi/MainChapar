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
        private readonly IWebHostEnvironment _env;
        public UsedBookController(ApplicationDbContext context, UserManager<User> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: UsedBook
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> MyBook()
        {
            var userId = _userManager.GetUserId(User);

            var myBooks = await _context.usedBooks
                .Where(b => b.UserId == userId)
                .Include(b => b.Images)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(myBooks);
        }

        [HttpGet]
        public IActionResult FilterUsedBooks(string sort)
        {
            var booksQuery = _context.usedBooks
                .Where(b => b.IsApproved) // فقط کتاب‌های تایید شده
                .Include(b => b.Images)
                .AsQueryable();

            switch (sort)
            {
                case "expensive":
                    booksQuery = booksQuery.OrderByDescending(b => b.Price);
                    break;
                case "cheap":
                    booksQuery = booksQuery.OrderBy(b => b.Price);
                    break;
                case "newest":
                    booksQuery = booksQuery.OrderByDescending(b => b.CreatedAt);
                    break;
                default:
                    booksQuery = booksQuery.OrderByDescending(b => b.Id);
                    break;
            }

            return PartialView("_UsedBookListPartial", booksQuery.ToList());
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
            //select list: user
            ViewData["UserId"] = new SelectList(_context.users, "Id", "Id");
            return View();
        }

        // POST: UsedBook/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsedBookCreateViewModel model)
        {

            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User);

            var usedBook = new UsedBook
            {
                Title = model.Title,
                Author = model.Author,
                Description = model.Description,
                Price = model.Price,
                ContactNumber = model.ContactNumber,
                CreatedAt = DateTime.Now,
                IsApproved = false,
                UserId = userId,
                Images = new List<UsedBookImage>()
            };

            //  ذخیره عکس اصلی
            if (model.MainImage != null && model.MainImage.Length > 0)
            {
                var mainFileName = Guid.NewGuid() + Path.GetExtension(model.MainImage.FileName);
                var mainFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/books", mainFileName);

                using (var stream = new FileStream(mainFilePath, FileMode.Create))
                {
                    await model.MainImage.CopyToAsync(stream);
                }
                
                usedBook.ImageName = "/uploads/books/" + mainFileName;
            }

            //  ذخیره سایر عکس‌ها
            if (model.Images != null)
            {
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
            }

            _context.usedBooks.Add(usedBook);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
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
        [HttpPost]
        public async Task<IActionResult> Edit(
    int id,
    [Bind("Id,Title,Author,Description,Price,ContactNumber,CreatedAt,IsApproved,UserId")] UsedBook usedBook,
    IFormFile MainImage,
    IFormFile[] GalleryImages)
        {
            if (id != usedBook.Id)
                return NotFound();

            var usedBookInDb = await _context.usedBooks
                .Include(u => u.Images)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usedBookInDb == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                // ویرایش فیلدهای اصلی
                usedBookInDb.Title = usedBook.Title;
                usedBookInDb.Author = usedBook.Author;
                usedBookInDb.Description = usedBook.Description;
                usedBookInDb.Price = usedBook.Price;
                usedBookInDb.ContactNumber = usedBook.ContactNumber;
                usedBookInDb.IsApproved = usedBook.IsApproved;

                // ذخیره عکس اصلی
                if (MainImage != null && MainImage.Length > 0)
                {
                    var newImageName = Guid.NewGuid().ToString() + Path.GetExtension(MainImage.FileName);
                    var newImagePath = Path.Combine(_env.WebRootPath, "uploads/books", newImageName);

                    using (var stream = new FileStream(newImagePath, FileMode.Create))
                    {
                        await MainImage.CopyToAsync(stream);
                    }

                    usedBookInDb.ImageName = newImageName;
                }
                else
                {
                    // اگر عکس اصلی جدیدی نیومده، همان عکس قبلی را نگه دار
                    usedBookInDb.ImageName = usedBookInDb.ImageName;
                }

                // ذخیره تصاویر گالری جدید
                if (GalleryImages != null && GalleryImages.Length > 0)
                {
                    foreach (var file in GalleryImages)
                    {
                        if (file.Length > 0)
                        {
                            var galleryFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var galleryPath = Path.Combine(_env.WebRootPath, "uploads/usedbooks", galleryFileName);

                            using (var stream = new FileStream(galleryPath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var usedBookImage = new UsedBookImage
                            {
                                UsedBookId = usedBookInDb.Id,
                                ImagePath = galleryFileName
                            };

                            _context.usedBookImages.Add(usedBookImage);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(usedBookInDb);
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

        public IActionResult DeleteGallery(int id)
        {
            var gallery = _context.usedBookImages.FirstOrDefault(x => x.Id == id);
            if (gallery == null)
            {
                return NotFound();
            }
            string d = Directory.GetCurrentDirectory();
            string fn = d + "\\wwwroot\\uploads\\books\\" + gallery.ImagePath;
            if (System.IO.File.Exists(fn))
            {
                System.IO.File.Delete(fn);
            }
            _context.usedBookImages.Remove(gallery);
            _context.SaveChanges();
            return Redirect("edit/" + gallery.UsedBookId);
        }


            private bool UsedBookExists(int id)
            {
                return _context.usedBooks.Any(e => e.Id == id);
            }
        
    }
}
