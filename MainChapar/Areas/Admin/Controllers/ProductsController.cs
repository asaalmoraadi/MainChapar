using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MainChapar.Data;
using MainChapar.Models;

namespace MainChapar.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,FullDesc,Price,Discount,ImageName,Qty,Tags,IsAvailable")] Product product, IFormFile? MainImage, IFormFile[]? GalleryImage)
        {
            if (ModelState.IsValid)
            {
                //=====================save image
                if (MainImage != null)
                {
                    product.ImageName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(MainImage.FileName);
                    string fn;
                    fn = Directory.GetCurrentDirectory();
                    string ImagePath = fn + @"\wwwroot\assets\img\product\" + product.ImageName;
                    using (var stream = new FileStream(ImagePath, FileMode.Create))
                    {
                        MainImage.CopyTo(stream);
                    }

                }
                //===================================
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                //===============save gallery image
                if (GalleryImage != null)
                {
                    foreach (var item in GalleryImage)
                    {
                        var newgallery = new ProductGallery();
                        newgallery.ProductId = product.Id;
                        //--------------
                        newgallery.ImageName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(item.FileName);
                        string fn;
                        fn = Directory.GetCurrentDirectory();
                        string ImagePath = fn + @"\wwwroot\assets\img\product\" + newgallery.ImageName;
                        using (var stream = new FileStream(ImagePath, FileMode.Create))
                        {
                            item.CopyTo(stream);
                        }

                        //----------------
                        _context.ProductGallerys.Add(newgallery);
                    }
                }
                await _context.SaveChangesAsync();
                //=============================
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            //-------------------
            ViewData["gallery"] = _context.ProductGallerys.Where(x => x.ProductId == product.Id).ToList();
            //----------------------
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,FullDesc,Price,Discount,ImageName,Qty,Tags,IsAvailable")] Product product, IFormFile? MainImage, IFormFile[]? GalleryImage)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //====================saveimage


                    if (MainImage != null)
                    {
                        string d = Directory.GetCurrentDirectory();
                        string fn = d + "\\wwwroot\\assets\\img\\product\\" + product.ImageName;
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }
                        using (var stream = new FileStream(fn, FileMode.Create))
                        {
                            MainImage.CopyTo(stream);
                        }
                    }

                    //=======================================

                    if (GalleryImage != null)
                    {
                        foreach (var item in GalleryImage)
                        {
                            var imagename = Guid.NewGuid() + Path.GetExtension(item.FileName);

                            //--------------
                            string d = Directory.GetCurrentDirectory();
                            string fn = d + "\\wwwroot\\assets\\img\\product\\" + imagename;

                            using (var stream = new FileStream(fn, FileMode.Create))
                            {
                                item.CopyTo(stream);
                            }

                            //----------------
                            var galleryitem = new ProductGallery()
                            {
                                ImageName = imagename,
                                ProductId = product.Id,
                            };
                            _context.ProductGallerys.Add(galleryitem);

                        }
                    }
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {

                //=====================delete image

                string d = Directory.GetCurrentDirectory();
                string fn = d + "\\wwwroot\\assets\\img\\product\\";
                string mainimagepath = fn + product.ImageName;
                if (System.IO.File.Exists(mainimagepath))
                {
                    System.IO.File.Delete(mainimagepath);
                }

                //--------------------------
                var galleries = _context.ProductGallerys.Where(x => x.ProductId == id).ToList();
                if (galleries != null)
                {
                    foreach (var item in galleries)
                    {
                        string galleryimagepath = fn + item.ImageName;


                        if (System.IO.File.Exists(galleryimagepath))
                        {
                            System.IO.File.Delete(galleryimagepath);
                        }
                    }
                    _context.ProductGallerys.RemoveRange(galleries);
                }
                //===========================
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
        public IActionResult DeleteGallery(int id)
        {
            var gallery = _context.ProductGallerys.FirstOrDefault(x => x.Id == id);
            if (gallery == null)
            {
                return NotFound();
            }
            string d = Directory.GetCurrentDirectory();
            string fn = d + "\\wwwroot\\assets\\img\\product\\" + gallery.ImageName;
            if (System.IO.File.Exists(fn))
            {
                System.IO.File.Delete(fn);
            }
            _context.ProductGallerys.Remove(gallery);
            _context.SaveChanges();
            return Redirect("edit/" + gallery.ProductId);

        }
    }
}
