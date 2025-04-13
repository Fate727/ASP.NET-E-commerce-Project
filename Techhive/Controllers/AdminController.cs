using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Techhive.Data;
using Techhive.Models;
using Techhive.ProductModels;
using System;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Techhive.TrendingModel;

namespace Techhive.Controllers
{
    [Authorize(Roles = "Admin")] // Ensures only users with the "Admin" role can access this controller
    public class AdminController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(UserManager<Users> userManager, SignInManager<Users> signInManager, AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Dashboard action: Redirects user to login page if they are not authenticated (handled by [Authorize] already)
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("User"))
                {
                    return RedirectToAction("Index", "Home");
                }
                return View();
            }
            return RedirectToAction("Login", "Account");
        }

        // ==============================
        //           BRAND SECTION
        // ==============================
        public IActionResult Add_Brand() => View();

        // ==============================
        //          ADD BRAND SECTION
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add_Brand(Brand model, IFormFile BrandImage)
        {
            if (_context.Brands.Any(b => b.Name == model.Name))
            {
                TempData["ErrorMessage"] = "Brand already exists!";
                return RedirectToAction("Add_Brand");
            }

            model.Image = await UploadImageAsync(BrandImage) ?? "/images/default.png";

            try
            {
                _context.Brands.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Brand added successfully!";
                return RedirectToAction("Brand_list");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database error: " + ex.Message);
                TempData["ErrorMessage"] = "Error saving the brand.";
                return RedirectToAction("Add_Brand");
            }
        }

        // ==============================
        //      BRAND LIST SECTION
        // ==============================
        public async Task<IActionResult> Brand_list(string name, int page = 1)
        {
            int pageSize = 6;
            var brandsQuery = _context.Brands.Include(b => b.Products).AsNoTracking();

            if (!string.IsNullOrEmpty(name))
            {
                brandsQuery = brandsQuery.Where(b => b.Name.Contains(name));
            }

            int totalBrands = await brandsQuery.CountAsync();
            var brands = await brandsQuery.OrderBy(b => b.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var model = new BrandListViewModel
            {
                Brands = brands,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalBrands / pageSize),
                SearchName = name
            };

            return View(model);
        }

        // ==============================
        //          EDIT BRAND SECTION
        // ==============================
        [HttpGet]
        public async Task<IActionResult> Edit_Brand(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            return brand == null ? NotFound() : View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit_Brand(Brand model, IFormFile BrandImage)
        {
            var brand = await _context.Brands.FindAsync(model.B_Id);
            if (brand == null) return NotFound();

            if (_context.Brands.Any(b => b.Name == model.Name && b.B_Id != model.B_Id))
            {
                TempData["ErrorMessage"] = "Brand name already exists!";
                return RedirectToAction("Edit_Brand", new { id = model.B_Id });
            }

            brand.Name = model.Name;
            var newImagePath = await UploadImageAsync(BrandImage);
            if (!string.IsNullOrEmpty(newImagePath))
            {
                DeleteImage(brand.Image);
                brand.Image = newImagePath;
            }

            try
            {
                _context.Brands.Update(brand);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Brand updated successfully!";
                return RedirectToAction("Brand_list");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database update failed: " + ex.Message);
                TempData["ErrorMessage"] = "Error saving the brand.";
                return RedirectToAction("Edit_Brand", new { id = model.B_Id });
            }
        }

        // ==============================
        //     DELETE BRAND SECTION
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete_Brand(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return NotFound();

            DeleteImage(brand.Image);
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Brand deleted successfully!";
            return RedirectToAction("Brand_list");
        }

        // ==============================
        //      IMAGE UPLOAD SECTION
        // ==============================
        private async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0) return null;

            try
            {
                string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images/brands");

                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                string filePath = Path.Combine(uploadPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return "/images/brands/" + fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Image upload failed: " + ex.Message);
                return null;
            }
        }

        private void DeleteImage(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath) && imagePath != "/images/default.png")
            {
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
        }

        // ==============================
        //       USER LOGOUT
        // ==============================
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "User");
        }


        // ==============================
        //       ADMIN DETAIL
        // ==============================
        [Route("Admin/Account_Detail")]
        public async Task<IActionResult> AdminDetail()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Pass the current user data to the view
            var userUpdateModel = new UserUpdate
            {
                FullName = user.FullName,
                Address = user.Address,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(userUpdateModel);
        }

        // ==============================
        //       UPDATE USER
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccountDetails(UserUpdate model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found. Please log in again.";
                return RedirectToAction("Login");
            }

            // Update Name, Phone Number, and Address
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to update account details.";
                return RedirectToAction("AdminDetail");
            }

            // Handle Password Change
            if (!string.IsNullOrEmpty(model.OldPassword) &&
                !string.IsNullOrEmpty(model.NewPassword) &&
                !string.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    TempData["ErrorMessage"] = "New passwords do not match.";
                    return RedirectToAction("AdminDetail");
                }

                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (!passwordChangeResult.Succeeded)
                {
                    TempData["ErrorMessage"] = "Incorrect old password or password does not meet requirements.";
                    return RedirectToAction("AdminDetail");
                }

                // Re-sign in the user after password change
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["SuccessMessage"] = "Account details updated successfully!";
            return RedirectToAction("AdminDetail");
        }


        // ==============================
        //       ADMIN FeedBack
        // ==============================
        public IActionResult Feedback(int page = 1)
        {
            int pageSize = 10;

            var feedbacks = _context.ContactMessages
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            var totalFeedbacks = feedbacks.Count;
            var pagedFeedbacks = feedbacks.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new FeedbackListViewModel
            {
                Feedbacks = pagedFeedbacks,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalFeedbacks / pageSize)
            };

            return View(viewModel);
        }


        public IActionResult ViewFeedback(int id)
        {
            var feedback = _context.ContactMessages.FirstOrDefault(f => f.Id == id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }
        // ==============================
        //       Trending FeedBack
        // ==============================
        public async Task<IActionResult> Trending(int page = 1, string search = "")
        {
            const int pageSize = 6;  // Define how many items per page

            // Search for products based on the search query
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            // Get the total number of products
            int totalProducts = await query.CountAsync();

            // Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            // Get products for the current page
            var products = await query
                .Include(p => p.Brands)
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
               .Take(pageSize)
                .ToListAsync();
            

            // Fetch the associated trending data
            var trending = await _context.Trendings
                .Where(t => products.Select(p => p.P_Id).Contains(t.P_Id))
                .ToListAsync();

            var model = new AdminTrendingModel
            {
                Products = products,
                Trend = trending,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchQuery = search
            };

            return View(model);
        }

        // ==============================
        //       Trending Detial
        // ==============================
        public async Task<IActionResult> ViewDetail(int id, int page = 1, string search = "")
        {
            int pageSize = 6;

            var product = await _context.Products
            .Include(p => p.Brands)
            .FirstOrDefaultAsync(p => p.P_Id == id);

            var query = _context.Recommendations
                .Include(r => r.User)
                .Where(r => r.P_Id == id);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.User.UserName.Contains(search));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var recommendations = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Extract users from the fetched recommendations
            var users = recommendations
                .Select(r => r.User)
                .Distinct()
                .ToList();

            var model = new TrendingDetailViewModel
            {
                Product = product,
                Recommendations = recommendations,
                User = users,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchQuery = search
            };

            return View(model);
        }

    }
}
