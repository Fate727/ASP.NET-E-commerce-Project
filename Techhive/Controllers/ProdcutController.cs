using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Techhive.Data;
using Techhive.Models;

namespace Techhive.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProductController> _logger;

        public ProductController(AppDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<ProductController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }


        // ==============================
        // ADD PRODUCT - GET
        // ==============================
        public IActionResult AddProduct()
        {
            PopulateBrand();
            return View();
        }

        // Populate the list of brands for the dropdown
        private void PopulateBrand()
        {
            ViewBag.BrandList = _context.Brands
                .Select(i => new SelectListItem { Text = i.Name, Value = i.B_Id.ToString() })
                .ToList();
        }

        // ==============================
        // ADD PRODUCT - POST
        // ==============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.BrandList = _context.Brands.Select(b => new SelectListItem
                {
                    Text = b.Name,
                    Value = b.B_Id.ToString()
                }).ToList();

                return View(model);
            }

            // Save images
            string image1Path = await SaveImageAsync(model.Image1, "products");
            string image2Path = model.Image2 != null ? await SaveImageAsync(model.Image2, "products") : null;
            string image3Path = model.Image3 != null ? await SaveImageAsync(model.Image3, "products") : null;

            // Create product entity with CreatedAt field
            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                Detail = model.Detail,
                Image1 = image1Path,
                Image2 = image2Path,
                Image3 = image3Path,
                B_Id = model.B_Id,
                CreatedAt = DateTime.UtcNow // Set the creation date
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product added successfully!";
            return RedirectToAction("ListProduct");
        }


        // Helper method to save images
        private async Task<string> SaveImageAsync(IFormFile image, string folderName)
        {
            if (image == null || image.Length == 0)
                return "/images/default.png"; // Return a default image if no image is uploaded

            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", folderName);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            string fileName = Guid.NewGuid() + Path.GetExtension(image.FileName); // Ensure unique file name
            string filePath = Path.Combine(uploadPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/" + folderName + "/" + fileName; // Return the relative path for storing in the database
        }


        // ==============================
        // LIST PRODUCTS WITH SEARCH & PAGINATION
        // ==============================
        public async Task<IActionResult> ListProduct(string search, int page = 1)
        {
            int pageSize = 6;

            // Build the query, including the related brand information.
            var query = _context.Products
                .Include(p => p.Brands)
                .AsQueryable();

            // If a search term is provided, filter the products by Name or Brand Name.
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    EF.Functions.Like(p.Name, $"%{search}%") ||
                    (p.Brands != null && EF.Functions.Like(p.Brands.Name, $"%{search}%"))
                );
            }

            // Get the total count for pagination
            int totalItems = await query.CountAsync();

            // Fetch the products for the current page (ordered by newest first)
            var products = await query
                .OrderByDescending(p => p.P_Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Create and populate the view model for the list
            var viewModel = new ProductListViewModel
            {
                Products = products,
                SearchQuery = search,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            return View(viewModel);
        }



        // ==============================
        // EDIT PRODUCT - GET
        // ==============================
        public async Task<IActionResult> EditProduct(int id)
        {
            // Fetch product data
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.P_Id == id);

            if (product == null)
            {
                return NotFound(); // Return 404 if product not found
            }

            // Populate the brand list for dropdown
            var model = new ProductViewModel
            {
                P_Id = product.P_Id,
                Name = product.Name,
                Price = product.Price,
                Detail = product.Detail,
                ExistingImage1 = product.Image1,
                ExistingImage2 = product.Image2,
                ExistingImage3 = product.Image3,
                B_Id = product.B_Id,
                BrandList = _context.Brands.Select(b => new SelectListItem
                {
                    Text = b.Name,
                    Value = b.B_Id.ToString()
                }).ToList() // Populating BrandList for the dropdown
            };

            return View(model);
        }


        // ==============================
        // EDIT PRODUCT - POST
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.BrandList = _context.Brands
                    .Select(b => new SelectListItem { Text = b.Name, Value = b.B_Id.ToString() })
                    .ToList();
                return View(model); // Return to the same page if validation fails
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.P_Id == model.P_Id);
            if (product == null)
            {
                return NotFound(); // Return 404 if product not found
            }

            // Update the product details
            product.Name = model.Name;
            product.Price = model.Price;
            product.Detail = model.Detail;
            product.B_Id = model.B_Id;
            product.UpdatedAt = DateTime.UtcNow; // Update modification timestamp

            // Handle Image1: Use existing image if no new image is uploaded
            if (model.Image1 != null)
                product.Image1 = await SaveImageAsync(model.Image1, "products");

            // Handle Image2: Use existing image if no new image is uploaded
            if (model.Image2 != null)
                product.Image2 = await SaveImageAsync(model.Image2, "products");

            // Handle Image3: Use existing image if no new image is uploaded
            if (model.Image3 != null)
                product.Image3 = await SaveImageAsync(model.Image3, "products");

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product updated successfully!";
            return RedirectToAction("ListProduct");
        }



        // ==============================
        // DELETE PRODUCT - POST
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // Find the product
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Delete images if they exist and are not default
            await DeleteProductImageAsync(product.Image1);
            await DeleteProductImageAsync(product.Image2);
            await DeleteProductImageAsync(product.Image3);

            // Remove product from database
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product deleted successfully!";
            return RedirectToAction("ListProduct");
        }


        // ==============================
        //      Delte images of product
        // ==============================
        private async Task DeleteProductImageAsync(string imagePath)
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

    }
}
