using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Techhive.Data;
using Techhive.ProductModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Techhive.Models;
using Techhive.TrendingModel;

namespace Techhive.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public CartController(UserManager<Users> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        // ==============================
        //          Add to cart
        // ==============================
        [HttpPost]
        public async Task<IActionResult> AddToCart(int P_Id, int Quantity = 1)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {

                return RedirectToAction("Login", "Account");
            }
            await UpdateAllTrendingScores();
            await TrackUserActivityAsync(userId, P_Id, "Addtocart");

            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.P_Id == P_Id && c.UserId == userId);

            if (existingCartItem != null)
            {
                // If the product already exists in the cart, update the quantity
                existingCartItem.Quantity += Quantity;
                _context.Carts.Update(existingCartItem);
            }
            else
            {
                // If the product does not exist in the cart, add it as a new item
                var cartItem = new Cart
                {
                    P_Id = P_Id,
                    UserId = userId,
                    Quantity = Quantity
                };
                await _context.Carts.AddAsync(cartItem);
            }

            // Save the changes to the database asynchronously
            await _context.SaveChangesAsync();

            // Set a success message in TempData
            TempData["SuccessMessage"] = "Product successfully added to cart!";

            return Redirect(Request.Headers["Referer"].ToString());
        }

        // ==============================
        //        Updatecart
        // ==============================
        [HttpPost]
        public IActionResult UpdateCart(Dictionary<int, int> quantity) // The key is Product.P_Id, value is the new quantity
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get UserId from the logged-in user

            // Get the cart items for the user
            var cartItems = _context.Carts
                                    .Where(c => c.UserId == userId)
                                    .Include(c => c.Product)
                                    .ToList();

            // Update cart items based on the form submission
            foreach (var item in cartItems)
            {
                if (quantity.ContainsKey(item.Product.P_Id))
                {
                    item.Quantity = quantity[item.Product.P_Id];
                }
            }

            _context.SaveChanges(); // Save changes to the database

            // Return the updated cart view
            return RedirectToAction("Cart"); // Redirect back to the cart page
        }


        // ==============================
        //          Remove cart
        // ==============================
        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get UserId from the logged-in user
            var cartItem = _context.Carts.FirstOrDefault(c => c.Product.P_Id == id && c.UserId == userId);

            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                _context.SaveChanges();
            }

            // Return a success response, not just a redirect
            return Ok();
        }


        // ==============================
        //       Update list cart
        // ==============================
        [HttpPost]
        public IActionResult UpdateQuantity([FromBody] CartUpdateModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get UserId from the logged-in user
            var cartItem = _context.Carts.FirstOrDefault(c => c.Product.P_Id == model.Id && c.UserId == userId);

            if (cartItem != null && model.Quantity > 0)
            {
                cartItem.Quantity = model.Quantity;
                _context.SaveChanges();
            }

            return Ok();
        }


        // ==============================
        //    Algorithm Implemetation
        // ==============================

        public async Task TrackUserActivityAsync(string userId, int productId, string type)
        {

            var existingRec = await _context.Recommendations
                .FirstOrDefaultAsync(r => r.UserID == userId && r.P_Id == productId && r.Type == type);

            if (existingRec != null)
            {

                existingRec.V_Count += 1;
                existingRec.UpdatedAt = DateTime.Now;

                _context.Recommendations.Update(existingRec);
            }
            else
            {
                var newRec = new Recommendation
                {
                    UserID = userId,
                    P_Id = productId,
                    Type = type,
                    V_Count = 1,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsOrdered = false
                };

                await _context.Recommendations.AddAsync(newRec);
            }

            // Now handle Trending table (track total views for the product)
            var existingTrending = await _context.Trendings
                .FirstOrDefaultAsync(t => t.P_Id == productId);

            if (existingTrending != null)
            {
                existingTrending.TotalViews += 1;
                existingTrending.TrendingScore += 1;
                existingTrending.LastUpdated = DateTime.Now;

                _context.Trendings.Update(existingTrending);
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

                await _context.Trendings.AddAsync(newTrending);
            }

            await _context.SaveChangesAsync();
        }



        // ==============================
        // Trending Algorithm Implemetation
        // ==============================
        public async Task UpdateAllTrendingScores()
        {
            var allTrendings = await _context.Trendings.ToListAsync(); // Get all trending products

            foreach (var trending in allTrendings)
            {
                TimeSpan timeSinceUpdate = DateTime.Now - trending.LastUpdated;
                double hours = timeSinceUpdate.TotalHours;
                double decay = Math.Pow(0.97, hours);

                // Update the trending score
                trending.TrendingScore = ((trending.TotalViews * 0.4) + (trending.OrderCount * 0.6)) * decay;
                trending.LastUpdated = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

    }

    }
