using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Techhive.Models;
using System.ComponentModel.DataAnnotations;

namespace Techhive.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;

        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        // Login - GET
        public IActionResult Login()
        {
            return View();
        }

        // Login - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {    
                    var signInResult = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                    if (signInResult.Succeeded)
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        else
                        {
                            return RedirectToAction("Index", "User");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid login attempt.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No user found with this email.");
                }
            }
            return View(model);
        }


        // Register - GET
        public IActionResult Register()
        {
            return View();
        }

        // Register - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Users
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.Name
                };

                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Assign "User" role by default
                    await userManager.AddToRoleAsync(user, "User");

                    // Sign in the user
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }


        public async Task<IActionResult> Account_Detail()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var userUpdateModel = new UserUpdate
            {
                FullName = user.FullName,
                Address = user.Address,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(userUpdateModel);
        }

        // Update Account Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccountDetails(UserUpdate model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found. Please log in again.";
                return RedirectToAction("Login");
            }

            // Update Name, Phone Number, and Address
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to update account details.";
                return RedirectToAction("Account_Detail");
            }

            // Handle Password Change
            if (!string.IsNullOrEmpty(model.OldPassword) &&
                !string.IsNullOrEmpty(model.NewPassword) &&
                !string.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    TempData["ErrorMessage"] = "New passwords do not match.";
                    return RedirectToAction("Account_Detail");
                }

                var passwordChangeResult = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (!passwordChangeResult.Succeeded)
                {
                    TempData["ErrorMessage"] = "Incorrect old password or password does not meet requirements.";
                    return RedirectToAction("Account_Detail");
                }

                // Re-sign in the user after password change
                await signInManager.RefreshSignInAsync(user);
            }

            TempData["SuccessMessage"] = "Account details updated successfully!";
            return RedirectToAction("Account_Detail");
        }



        // Logout user
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "User");
        }

    }
}
