using MainChapar.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MainChapar.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var roles = _roleManager.Roles.Select(P => new RoleDTO
            {
                Id = P.Id,
                Name = P.Name,
            }).ToList();
            return View(roles); 
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(RoleDTO role)
        {
            IdentityRole role1 = new IdentityRole()
            {
                Name = role.Name,
            };
            var result = _roleManager.CreateAsync(role1).Result;
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            return View(role);
        }
    }
}
