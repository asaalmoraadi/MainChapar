using MainChapar.Data;
using MainChapar.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MainChapar.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ????? ?????
            var banners = _context.Banners.ToList();
            ViewData["banners"] = banners;

            // ????? ????? 10 ????
            var books = await _context.usedBooks
                .Include(b => b.Images)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(books);
        }

        public IActionResult AboutUs()
        {
            return View();
        }
        public IActionResult Contact() 
        {
            return View();
        
        }
        [HttpPost]
        public IActionResult Contact(Contact contact)
        {
            _context.contacts.Add(contact);
            _context.SaveChanges();
            return View(contact);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
