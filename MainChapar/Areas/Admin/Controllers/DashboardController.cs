using MainChapar.Data;
using MainChapar.Models;
using MainChapar.ViewModel.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainChapar.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var totalPrintServices = _context.PrintRequests.Count();
            var completedPrintServices = _context.PrintRequests.Count(ps => ps.Status == "Completed" && ps.IsFinalized);
            var totalOrders = _context.orders.Count();

            //محصولات
            var totalProductRevenue = _context.ordersDetail
                .Sum(od => od.Quantity * od.UnitPrice);

            //خدمات چاپ
            var totalPrintRevenue = _context.PrintRequests
                .Where(pr => pr.Status == "Completed" && pr.IsFinalized)
                .Sum(pr => (decimal?)pr.TotalPrice) ?? 0;

            var totalRevenue = totalProductRevenue + totalPrintRevenue;

           
            ViewBag.TotalServices = totalPrintServices;
            ViewBag.CompletedServices = completedPrintServices;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue.ToString("N0");

            return View();
        }
        
    }
}
