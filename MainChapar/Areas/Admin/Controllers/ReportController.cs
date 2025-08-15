using MainChapar.Data;
using MainChapar.ViewModel.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainChapar.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SalesStats()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            // ----------------------------
            // محصولات فیزیکی (Order)
            // ----------------------------
            var confirmedOrders = _context.orders
                .Include(o => o.OrderDetails)
                .Where(o => o.IsConfirmed)
                .ToList();

            var todayProductOrders = confirmedOrders.Where(o => o.CreatedAt.Date == today);
            var monthProductOrders = confirmedOrders.Where(o => o.CreatedAt >= monthStart && o.CreatedAt <= today);

            var allProductOrdersThisYear = confirmedOrders
                .Where(o => o.CreatedAt.Year == today.Year)
                .ToList();

            // ----------------------------
            // خدمات چاپ (PrintRequest)
            // ----------------------------
            var finalizedPrints = _context.PrintRequests
            .Where(p => p.Status == "Completed" && p.IsFinalized)
            .ToList();

            var todayPrints = finalizedPrints.Where(p => p.CreatedAt.Date == today);
            var monthPrints = finalizedPrints.Where(p => p.CreatedAt >= monthStart && p.CreatedAt <= today);
            var allPrintsThisYear = finalizedPrints
                .Where(p => p.CreatedAt.Year == today.Year)
                .ToList();

            // ----------------------------
            // ساخت ViewModel
            // ----------------------------

            var model = new SalesStatsViewModel
            {
                // امروز
                TodayProductCount = todayProductOrders.Count(),
                TodayProductSales = todayProductOrders.Sum(o => o.TotalPrice),
                TodayPrintCount = todayPrints.Count(),
                TodayPrintSales = todayPrints.Sum(p => p.TotalPrice ?? 0),

                // این ماه
                MonthProductCount = monthProductOrders.Count(),
                MonthProductSales = monthProductOrders.Sum(o => o.TotalPrice),
                MonthPrintCount = monthPrints.Count(),
                MonthPrintSales = monthPrints.Sum(p => p.TotalPrice ?? 0),

                // آمار ماهانه
                MonthlySales = Enumerable.Range(1, 12)
                    .Select(month => new MonthlySalesItem
                    {
                        Month = month,
                        ProductCount = allProductOrdersThisYear.Count(o => o.CreatedAt.Month == month),
                        ProductSales = allProductOrdersThisYear
                            .Where(o => o.CreatedAt.Month == month)
                            .Sum(o => o.TotalPrice),

                        PrintCount = allPrintsThisYear.Count(p => p.CreatedAt.Month == month),
                        PrintSales = allPrintsThisYear
                            .Where(p => p.CreatedAt.Month == month)
                            .Sum(p => p.TotalPrice ?? 0)
                    })
                    .ToList()
            };

            return View(model);
        }
    }
}
