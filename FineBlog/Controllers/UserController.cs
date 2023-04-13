using AspNetCoreHero.ToastNotification.Abstractions;
using FineBlog.Models;
using FineBlog.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;

namespace FineBlog.Areas.Admin.Controllers
{

    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private RoleManager<IdentityRole> _roleManager;
        public INotyfService _notification { get; }
        public UserController(UserManager<ApplicationUser> userManager, 
                                    SignInManager<ApplicationUser> signInManager, 
                                    INotyfService notyfService, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _notification = notyfService;
            _roleManager = roleManager;
        }

        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);

            var vm = new UserVM()
            {
                Id= loggedInUser.Id,
                FirstName = loggedInUser.FirstName,
                LastName = loggedInUser.LastName,
                UserName = loggedInUser.UserName,
                Email = loggedInUser.Email,
                ThumbnailUrl= loggedInUser.ThumbnailUrl,
            };

            return View(vm);
        }

        
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                _notification.Error("User does not exsits");
                return View();
            }
            var vm = new ResetPasswordVM()
            {
                Id = existingUser.Id,
                UserName = existingUser.UserName,
                ThumbnailUrl= existingUser.ThumbnailUrl
            };
            return View(vm);
        }

        
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM vm)
        {
            if (!ModelState.IsValid)
            { 
                return View(vm); 
            }
            var existingUser = await _userManager.FindByIdAsync(vm.Id);
            if (existingUser == null)
            {
                _notification.Error("User does not exist");
                return View(vm);
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
            var result = await _userManager.ResetPasswordAsync(existingUser, token, vm.NewPassword);
            if (result.Succeeded)
            {
                _notification.Success("Password reset successful");
                return RedirectToAction("Index");
            }
            return View(vm);
        }


        
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterVM());
        }

        
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) 
            { 
                return View(vm); 
            }
            var checkUserByEmail = await _userManager.FindByEmailAsync(vm.Email);
            if (checkUserByEmail != null)
            {
                _notification.Error("Email already exists");
                return View(vm);
            }
            var checkUserByUsername = await _userManager.FindByNameAsync(vm.UserName);
            if (checkUserByUsername != null)
            {
                _notification.Error("Username already exists");
                return View(vm);
            }

           
            var applicationUser = new ApplicationUser()
            {
                Email = vm.Email,
                UserName = vm.UserName,
                FirstName = vm.FirstName,
                LastName = vm.LastName
            };
            IdentityRole role = await _roleManager.FindByNameAsync("Author");
            if (role == null)
            {
                IdentityResult _result = await _roleManager.CreateAsync(new IdentityRole("Author"));
                if (!_result.Succeeded)
                {
                    Errors(_result);
                }
            }
            var result =   await _userManager.CreateAsync(applicationUser,vm.Password);
            if (result.Succeeded)
            {
                string code = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

                await _userManager.AddToRoleAsync(applicationUser, "Author");
                _notification.Success("User registered successfully");
                return RedirectToAction("Login");
            }
            else
            {
                _notification.Error("Please choose a stronger password. Your password must contain at least 6 characters and include a combination of uppercase and lowercase letters, numbers, and special characters.");
            }
            return View(vm);
        }


        [HttpGet("Login")]
        public IActionResult Login()
        {
            if (!HttpContext.User.Identity!.IsAuthenticated)
            {
                return View(new LoginVM());
            }
            return RedirectToAction("Index", "Post");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid) 
            { 
                return View(vm); 
            }
            ApplicationUser appUser = await _userManager.FindByEmailAsync(vm.Email);
            var existingMail = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == vm.Email);
            if (existingMail == null)
            {
                _notification.Error("Email does not exist");
                return View(vm);
            }
            var verifyPassword = await _userManager.CheckPasswordAsync(appUser, vm.Password);
            if (!verifyPassword)
            {
                _notification.Error("Invalid Password");
                return View(vm);
            }
            await _signInManager.PasswordSignInAsync(appUser, vm.Password, vm.RememberMe, true);
            _notification.Success("Login Successful");
            return RedirectToAction("Index", "Post");

        }

        [HttpPost]
        
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            _notification.Success("You are logged out successfully");
            return RedirectToAction("Index", "Home");
        }
        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                TempData["Error"] = $"{error.Code} - {error.Description}";
            }
        }
        [HttpGet("AccessDenied")]
        
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
