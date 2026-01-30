using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.ViewModels;

namespace ToursAndTravelsManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            Log.Information("Register page accessed.");
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth,
                    Address = model.Address,
                    City = model.City,
                    Country = model.Country,
                    RegistrationDate = DateTime.Now,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    Log.Information("User {Email} registered successfully.", user.Email);

                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        var role = new IdentityRole(model.Role);
                        var roleResult = await _roleManager.CreateAsync(role);
                        if (roleResult.Succeeded)
                        {
                            Log.Information("Role {Role} created successfully.", model.Role);
                        }
                        else
                        {
                            Log.Error("Role creation failed for {Role}.", model.Role);
                        }
                    }

                    await _userManager.AddToRoleAsync(user, model.Role);
                    Log.Information("User {Email} assigned to role {Role}.", user.Email, model.Role);

                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    Log.Information("User {Email} signed in after registration.", user.Email);

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    Log.Error("Registration error for {Email}: {Error}", user.Email, error.Description);
                }
            }

            return View(model);
        }

        // GET: Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            Log.Information("Login page accessed.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    Log.Information("User {Email} logged in successfully.", model.Email);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    Log.Warning("Invalid login attempt for {Email}.", model.Email);
                    return View(model);
                }
            }

            Log.Warning("Login failed due to invalid model state.");
            return View(model);
        }


        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            Log.Information("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                Log.Information("Redirecting to local URL: {ReturnUrl}.", returnUrl);
                return Redirect(returnUrl);
            }
            else
            {
                Log.Information("Redirecting to home page.");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
