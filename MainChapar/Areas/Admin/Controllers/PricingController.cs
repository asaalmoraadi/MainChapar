using MainChapar.Data;
using MainChapar.Models;
using MainChapar.ViewModel.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainChapar.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PricingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PricingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // نمایش همه قیمت‌ها
        public async Task<IActionResult> Index()
        {
            var prices = await _context.printPricings.ToListAsync();
            return View(prices);
        }

        public IActionResult CreatePricing()
        {
            // چون لیست‌ها را حذف کردیم، اینجا فقط ویو مدل خالی می‌فرستیم
            return View(new PrintPricingCreateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreatePricing(PrintPricingCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // چون لیست نداریم نیازی به مقداردهی مجدد نیست
                return View(vm);
            }

            var pricing = new PrintPricing
            {
                PrintType = vm.PrintType,
                PaperType = vm.PaperType,
                PaperSize = vm.PaperSize,
                IsDoubleSided = vm.IsDoubleSided,
                PricePerPage = vm.PricePerPage,
                IsAvailable = vm.IsAvailable
            };

            _context.Add(pricing);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Pricing", new { area = "Admin" });
        }
    }
}
