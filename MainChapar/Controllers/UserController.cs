using MainChapar.Models.DTOs;
using MainChapar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MainChapar.Models.DTO;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            //List of all users
            var users = _userManager.Users.ToList();
            var userRolesDTOs = new List<UserWithRolesDTO>();

            foreach (var user in users)
            {
                //Get Role(user)
                var roles = await _userManager.GetRolesAsync(user);
                userRolesDTOs.Add(new UserWithRolesDTO
                {
                    Id = user.Id,
                    Username = user.UserName,
                    FirstName = user.Name,
                    LastName = user.LName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    CurrentRole = roles.FirstOrDefault() ?? "user",
                    AllRoles = new List<string> { "admin", "user" } // برای dropdown
                });
            }

            return View(userRolesDTOs);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newRole))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "خطا در حذف نقش‌های قبلی.");
                return RedirectToAction("Index");
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "خطا در افزودن نقش جدید.");
            }

            return RedirectToAction("Index");
        }
        public IActionResult Register()
        {
            return View(new UserDTO());
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
                PhoneNumber = register.PhoneNumber,
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
         
                var roles = _userManager.GetRolesAsync(user).Result;
                System.Diagnostics.Debug.WriteLine("نقش‌های کاربر: " + string.Join(", ", roles));

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
                p.PhoneNumber
            });
            return View(users);
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            return View(user);
        }

        public IActionResult AccessDenied()
        {
            //اگر کاربر با دسترسی اشتباهی وارد صفحه شود اخطار می دهد
            return View(); 
        }
    }
}
