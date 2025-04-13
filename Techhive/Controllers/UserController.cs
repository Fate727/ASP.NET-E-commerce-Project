using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Techhive.Data;
using Techhive.Models;
using Techhive.ProductModels;
using Techhive.HomeModel;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Techhive.TrendingModel;

namespace Techhive.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _db;

        // Constructor that injects AppDbContext
        public UserController(UserManager<Users> userManager, AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        // ==============================
        //       User Home SECTION
        // ==============================

        public IActionResult Index()
        {
            // Fetch all brands
            var brands = _db.Brands.ToList();

            // Fetch 8 latest products (sorted by date)
            var latestProducts = _db.Products
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToList();

            // Get top 2 most viewed recommended products
            var recommendProducts = _db.Products
                .Join(
                    _db.Trendings,
                    p => p.P_Id,
                    r => r.P_Id,
                    (p, r) => new { Product = p, VCount = r.TotalViews }
                )
                .OrderByDescending(x => x.VCount)
                .Select(x => x.Product)
                .Distinct()
                .Take(2)
                .ToList();

            // Get top 8 featured products (recommended first)
            var recommendedProducts = _db.Products
                .Join(
                    _db.Recommendations,
                    p => p.P_Id,
                    r => r.P_Id,
                    (p, r) => new { Product = p, VCount = r.V_Count }
                )
                .OrderByDescending(x => x.VCount)
                .Select(x => x.Product)
                .Distinct()
                .Take(8)
                .ToList();

            // If less than 8, fill with other products
            int remainingCount = 8 - recommendedProducts.Count;
            if (remainingCount > 0)
            {
                var recommendedIds = recommendedProducts.Select(p => p.P_Id).ToList();

                var fallbackProducts = _db.Products
                    .Where(p => !recommendedIds.Contains(p.P_Id))
                    .OrderBy(p => p.Name)
                    .Take(remainingCount)
                    .ToList();

                recommendedProducts.AddRange(fallbackProducts);
            }

            var viewModel = new Techhive.HomeModel.ProductViewModel
            {
                Brands = brands,
                LatestProducts = latestProducts,
                FeaturedProducts = recommendedProducts,
                RecommendProducts = recommendProducts
            };

            return View(viewModel);
        }


        // ==============================
        //     Prodcut Detail Section 
        // ==============================
        public async Task<IActionResult> ProductDetails(int id)
        {

            var product = await _db.Products
                .Include(p => p.Brands) 
                .FirstOrDefaultAsync(p => p.P_Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId))
            {

                await UpdateAllTrendingScores();

                await TrackUserActivityAsync(userId, id, "view");
            }

            var relatedProducts = await _db.Products
                .Where(p => p.B_Id == product.B_Id && p.P_Id != id)
                .Take(6)
                .ToListAsync();

            var viewModel = new RelatedProductViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts 
            };

            return View(viewModel);
        }



        // ==============================
        //      Shop Section 
        // ==============================
        public IActionResult Shop(int page = 1, string search = "", List<int> brandIds = null, string price_range = "", string sort = "")
        {
            var minPrice = _db.Products.Min(p => p.Price);
            var maxPrice = _db.Products.Max(p => p.Price);

            decimal? selectedMinPrice = null, selectedMaxPrice = null;
            if (!string.IsNullOrEmpty(price_range))
            {
                var prices = price_range.Split(',');
                if (prices.Length == 2)
                {
                    selectedMinPrice = decimal.Parse(prices[0]);
                    selectedMaxPrice = decimal.Parse(prices[1]);
                }
            }

            var query = _db.Products.AsQueryable();

            if (brandIds != null && brandIds.Any())
            {
                query = query.Where(p => brandIds.Contains(p.B_Id));
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            if (selectedMinPrice.HasValue && selectedMaxPrice.HasValue)
            {
                query = query.Where(p => p.Price >= selectedMinPrice.Value && p.Price <= selectedMaxPrice.Value);
            }

            // Apply sorting
            query = ApplySorting(query, sort);

            var pageSize = 9;
            var totalProducts = query.Count();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var products = query.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            var viewModel = new ShopViewModel
            {
                Products = products,
                TotalProducts = totalProducts,
                Brands = _db.Brands.ToList(),
                SearchQuery = search,
                BrandIds = brandIds ?? new List<int>(),
                PriceRange = price_range,
                CurrentPage = page,
                TotalPages = totalPages,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            return View(viewModel);
        }



        // ==============================
        //     List cart SECTION
        // ==============================

        public IActionResult Cart()
        {
            // Get the userId from the currently authenticated user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // This fetches the UserId (GUID or string ident   ifier)

            // Retrieve cart items for the current user based on the UserId
            var cartItems = _db.Carts
                                .Where(c => c.UserId == userId)  // Use userId here to match the correct user
                                .Include(c => c.Product)
                                .ToList();

            // Calculate the total price
            var totalPrice = cartItems.Sum(item => item.Product.Price * item.Quantity);

            // Return the cart view with the list of cart items and the total price
            var viewModel = new CartViewModel
            {
                CartItems = cartItems,
                TotalPrice = totalPrice
            };

            return View(viewModel);
        }




        // ==============================
        //          About SECTION
        // ==============================

        public IActionResult About()
        {
            return View();
        }



        // ==============================
        //       About Contact
        // ==============================
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMessage model)
        {
            if (ModelState.IsValid)
            {
                _db.ContactMessages.Add(model);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thank you! Your message has been received.";
                return RedirectToAction("Contact");
            }

            return View(model);
        }


        // ==============================
        //    Algorithm Implemetation
        // ==============================

        public async Task TrackUserActivityAsync(string userId, int productId, string type)
        {

            var existingRec = await _db.Recommendations
                .FirstOrDefaultAsync(r => r.UserID == userId && r.P_Id == productId && r.Type == type);

            if (existingRec != null)
            {

                existingRec.V_Count += 1;
                existingRec.UpdatedAt = DateTime.Now;

                _db.Recommendations.Update(existingRec);
            }
            else
            {
                var newRec = new Recommendation
                {
                    UserID = userId,
                    P_Id = productId,
                    Type = type, // "view", "addtocart".
                    V_Count = 1,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsOrdered = false
                };

                await _db.Recommendations.AddAsync(newRec);
            }

            // Now handle Trending table (track total views for the product)
            var existingTrending = await _db.Trendings
                .FirstOrDefaultAsync(t => t.P_Id == productId);

            if (existingTrending != null)
            {
                existingTrending.TotalViews += 1;
                existingTrending.TrendingScore += 1;
                existingTrending.LastUpdated = DateTime.Now;

                _db.Trendings.Update(existingTrending);
            }
            else
            {
                var newTrending = new Trending
                {
                    P_Id = productId,
                    TotalViews = 1,
                    TrendingScore = 1,
                    OrderCount = 0,
                    LastUpdated = DateTime.Now
                };

                await _db.Trendings.AddAsync(newTrending);
            }

            await _db.SaveChangesAsync();
        }

        // ==============================
        //  Trending Algorithm Implemetation
        // ==============================
        public async Task UpdateAllTrendingScores()
        {
            var allTrendings = await _db.Trendings.ToListAsync(); // Get all trending products

            foreach (var trending in allTrendings)
            {
                TimeSpan timeSinceUpdate = DateTime.Now - trending.LastUpdated;
                double hours = timeSinceUpdate.TotalHours;
                double decay = Math.Pow(0.97, hours);

                // Update the trending score
                trending.TrendingScore = ((trending.TotalViews * 0.4) + (trending.OrderCount * 0.6)) * decay;
                trending.LastUpdated = DateTime.Now;
            }

            await _db.SaveChangesAsync(); 
        }



        // ==============================
        //    Algorithm Lisitng
        // ==============================
        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string sort)
        {
            switch (sort)
            {
                case "1": // Featured - based on T_Views (Trending views)
                    return ApplyTrending(query);  
                
                case "2": // Recommedation - based on T_Views (Trending views)
                    return ApplyRecommendation(query);

                case "3": // Best Selling - based on P_Views (Ordered popularity)
                    return ApplyPopular(query);

                case "4": // Alphabetically A-Z
                    return query.OrderBy(p => p.Name);

                case "5": // Alphabetically Z-A
                    return query.OrderByDescending(p => p.Name);

                case "6": // Price low to high
                    return query.OrderBy(p => p.Price);

                case "7": // Price high to low
                    return query.OrderByDescending(p => p.Price);

                case "8": // Date old to new
                    return query.OrderBy(p => p.CreatedAt);

                case "9": // Date new to old
                    return query.OrderByDescending(p => p.CreatedAt);

                default:
                    return query.OrderBy(p => p.Name);
            }
        }


        private IQueryable<Product> ApplyTrending(IQueryable<Product> query)
        {

            var trendingQuery = from p in query
                                join t in _db.Trendings on p.P_Id equals t.P_Id into pt
                                from t in pt.DefaultIfEmpty()
                                orderby t.TrendingScore descending // Order by TrendingScore from Trending table
                                select p;

            return trendingQuery;
        }

        private IQueryable<Product> ApplyRecommendation(IQueryable<Product> query)
        {
            var bestRecommendationQuery = from p in query
                                          join t in _db.Recommendations on p.P_Id equals t.P_Id into pt
                                          from t in pt.DefaultIfEmpty()
                                          orderby t.V_Count descending
                                          select p;

            var recommendedProducts = bestRecommendationQuery.Take(5); // Adjust count if needed

            if (!recommendedProducts.Any())
                return query.OrderBy(p => p.Name);

            var otherProducts = query.Except(recommendedProducts);
            return recommendedProducts.Concat(otherProducts);
        }

        private IQueryable<Product> ApplyPopular(IQueryable<Product> query)
        {
            var bestSellingQuery = from p in query
                                   join t in _db.Trendings on p.P_Id equals t.P_Id into pt
                                   from t in pt.DefaultIfEmpty()
                                   orderby t.OrderCount descending
                                   select p;

            var topSelling = bestSellingQuery.Take(5); // Adjust count if needed

            if (!topSelling.Any())
                return query.OrderBy(p => p.Name);

            var remaining = query.Except(topSelling);
            return topSelling.Concat(remaining);
        }

    }
}
