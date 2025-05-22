using MainChapar.Models.DTOs;
using MainChapar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MainChapar.Models.DTO;

namespace MainChapar.Controllers
{
    public class UserController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public UserController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserDTO register)
        {
            if (!ModelState.IsValid)
            {
                return View(register);
            }

            // ایجاد کاربر جدید
            var newUser = new User
            {
                UserName = register.Username,
                Email = register.Email,
                Name = register.FirstName,
                LName = register.LastName,
            };

            // ذخیره کاربر با استفاده از UserManager
            var result = await _userManager.CreateAsync(newUser, register.Password);

            if (!result.Succeeded)
            {
                // اگر ایجاد کاربر با شکست مواجه شد
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(register);
            }

            // پیدا کردن کاربر ایجاد شده
            var usr = await _userManager.FindByNameAsync(register.Username);
            if (usr == null)
            {
                ModelState.AddModelError("", "کاربر ایجاد شده پیدا نشد.");
                return View(register);
            }

            // اضافه کردن نقش به کاربر
            var result2 = await _userManager.AddToRoleAsync(usr, "user");

            if (result2.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            // در صورتی که اضافه کردن نقش به کاربر با شکست مواجه شد
            foreach (var error in result2.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(register);
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();

        }

        [HttpPost]
        public IActionResult Login(LoginDTO loginUser)
        {
            if (!ModelState.IsValid)
            {
                return View(loginUser); // ارسال مدل LoginDTO به ویو
            }

            var user = _userManager.FindByNameAsync(loginUser.UserName).Result;
            if (user == null)
            {
                ModelState.AddModelError("", "نام کاربری وجود ندارد.");
                return View(loginUser);
            }

            var result = _signInManager.PasswordSignInAsync(user, loginUser.Password, true, true).Result;

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "نام کاربری یا رمز عبور اشتباه است.");
            return View(loginUser);
        }

        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("Login", "User");
        }

        public IActionResult UserList()
        {
            var users = _userManager.Users.Select(p => new
            {
                p.Id,
                p.Name,
                p.Email,
                p.LName,
                p.UserName,
            });
            return View(users);
        }
        
    }
}
