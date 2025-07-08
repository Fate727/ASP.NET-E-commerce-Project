using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Techhive.Data; 
using Techhive.Models;
using Techhive.ProductModels;
using Techhive.OrderModels;
using Techhive.TrendingModel;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Numerics;

namespace Techhive.Controllers
{
    public class OrderController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;

        public OrderController(UserManager<Users> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // ==============================
        //       Checkoutview SECTION
        // ==============================
        public IActionResult Checkout()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            decimal subtotal = cartItems.Sum(c => c.Product.Price * c.Quantity);
            decimal total = subtotal;

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            var model = new CheckoutViewModel
            {
                FullName = user?.FullName,
                Phone = user?.PhoneNumber,
                Address = user?.Address,
                CartItems = cartItems,
                Subtotal = subtotal,
                Total = total
            };

            return View(model);
        }

        // ==============================
        //       Checkout order SECTION
        // ==============================

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Cart", "Shop");
            }

            decimal total = cartItems.Sum(c => c.Product.Price * c.Quantity);
            var user = await _userManager.FindByIdAsync(userId);

            // Update user address and phone if provided in checkout
            bool userUpdated = false;

            if (!string.IsNullOrWhiteSpace(model.Address))
            {
                user.Address = model.Address;  // Assuming your custom user class has Address property
                userUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(model.Phone))
            {
                user.PhoneNumber = model.Phone;  // IdentityUser has PhoneNumber property
                userUpdated = true;
            }

            if (userUpdated)
            {
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    TempData["Error"] = "Failed to update user information.";
                    return RedirectToAction("Checkout");
                }
            }

            if (model.PaymentMethod == "Khalti")
            {
                var url = "https://dev.khalti.com/api/v2/epayment/initiate/";

                var payload = new
                {
                    return_url = "https://localhost:7239/order/khalti-success",
                    website_url = "https://localhost:7239/",
                    amount = (int)(total * 100),  // amount in paisa
                    purchase_order_id = Guid.NewGuid().ToString(),
                    purchase_order_name = "TechHive Order",
                    customer_info = new
                    {
                        name = string.IsNullOrWhiteSpace(model.FullName) ? user?.UserName : model.FullName,
                        email = string.IsNullOrWhiteSpace(user.Email) ? user?.Email : user.Email,
                        phone = string.IsNullOrWhiteSpace(model.Phone) ? "0000000000" : model.Phone
                    }
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "key live_secret_key_68791341fdd94846a146f0457ff7b455"); // Your Khalti live secret key here

                var response = await client.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                    return Redirect((string)jsonResponse.payment_url);
                }
                else
                {
                    TempData["Error"] = "Failed to initiate Khalti payment.";
                    return RedirectToAction("Checkout", "Order");
                }
            }
            else if (model.PaymentMethod == "CashOnDelivery")
            {
                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = total,
                    ShippingAddress = model.Address,
                    PaymentMethod = model.PaymentMethod,
                    OrderDate = DateTime.Now,
                    Status = "Pending"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        P_Id = item.P_Id,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    });
                }

                await _context.SaveChangesAsync();
                _context.Carts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
            }

            TempData["Error"] = "Invalid payment method.";
            return RedirectToAction("Checkout");
        }



        // ==============================
        //      Order Confrim SECTION
        // ==============================
        public IActionResult OrderConfirmation(int orderId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Products)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }



        // ==============================
        //    User Order list SECTION
        // ==============================
        public IActionResult OrderList(int page = 1, int pageSize = 10)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the total number of orders for the user
            var totalOrders = _context.Orders
                .Where(o => o.UserId == userId)
                .Count();

            // Get paginated orders for the user and include the User
            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.User) // Ensure the User property is loaded
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Products)
                .Skip((page - 1) * pageSize)  // Skip the previous pages
                .Take(pageSize)  // Take the current page size
                .ToList();

            // Calculate total pages for pagination
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            var viewModel = new OrderListViewModel
            {
                Orders = orders,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }



        // ==============================
        //    User Order Detial SECTION
        // ==============================

        [HttpGet]
        [Route("Order/OrderDetail/{orderId}")]
        public IActionResult OrderDetail(int orderId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch the order along with the related User to avoid null reference
            var order = _context.Orders
                                .Include(o => o.User)  // Eager loading the User
                                .Where(o => o.OrderId == orderId && o.UserId == userId)  // Ensure the order belongs to the logged-in user
                                .FirstOrDefault();

            if (order == null)
            {
                return NotFound();
            }

            var orderItems = _context.OrderItems
                                     .Where(oi => oi.OrderId == orderId)
                                     .ToList();

            var productIds = orderItems.Select(oi => oi.P_Id).ToList();
            var products = _context.Products
                                   .Where(p => productIds.Contains(p.P_Id))
                                   .ToList();

            var orderDetailModel = new OrderDetialModel
            {
                Orders = new List<Order> { order },
                OrderItems = orderItems,
                Products = products
            };

            return View(orderDetailModel);
        }



        // ==============================
        //  User cancel Order req SECTION
        // ==============================

        [HttpPost]
        public async Task<IActionResult> RequestCancel(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                TempData["SuccessMessage"] = "Order not found.";
                return RedirectToAction("MyOrders");
            }

            if (order == null || (order.Status != "Processing" && order.Status != "Pending"))
            {
                TempData["SuccessMessage"] = "You can only cancel orders that are Processing or Pending.";
                return RedirectToAction("MyOrders");
            }

            if (order.Status == "Pending")
            {
                order.Status = "Cancelled";
                TempData["SuccessMessage"] = "Your order has been cancelled successfully.";
            }
            else if (order.Status == "Processing")
            {
                if (!order.CancelRequested)
                {
                    order.CancelRequested = true;
                    TempData["SuccessMessage"] = "Your cancel request has been submitted.";
                }
                else
                {
                    TempData["SuccessMessage"] = "You have already requested to cancel this order.";
                }
            }
            else
            {
                TempData["SuccessMessage"] = "You can only cancel orders that are Pending or Processing.";
            }

            _context.Update(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("OrderDetail", "Order", new { id = orderId });
        }


        // ==============================
        //    Admin Order List
        // ==============================

        public IActionResult adminOrderList(string searchTerm, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(o =>
                    o.User.FullName.Contains(searchTerm) ||
                    o.User.PhoneNumber.Contains(searchTerm) ||
                    o.OrderId.ToString().Contains(searchTerm)
                );
            }

            int totalOrders = query.Count();

            var orders = query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new AdminOrderListViewModel
            {
                Orders = orders,
                SearchTerm = searchTerm,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize),
                PageSize = pageSize
            };

            return View(viewModel);
        }



        // ==============================
        //    Admin Order Detial
        // ==============================

        public IActionResult adminOrderDetail(int id)
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Products)
                .Where(o => o.OrderId == id)
                .ToList();

            var orderItems = _context.OrderItems
                .Include(oi => oi.Products)
                .Where(oi => oi.OrderId == id)
                .ToList();

            var products = _context.Products
                .Include(p => p.Brands) // Ensure brands are included as well
                .ToList();

            decimal totalAmount = orders.Sum(o => o.TotalAmount);

            string orderStatus = orders.FirstOrDefault()?.Status ?? "Pending";  // Set the order status, defaulting to "Pending" if not found

            int totalOrders = _context.Orders.Count();

            var orderDetailViewModel = new AdminOrderDetailViewModel
            {
                Orders = orders,
                OrderItems = orderItems,
                Products = products,
                TotalAmount = totalAmount,
                TotalOrders = totalOrders,
                Status = orderStatus  // Assign the status here
            };

            return View(orderDetailViewModel);
        }

        // ==============================
        //    Admin Order Update
        // ==============================

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order != null)
            {
                if (status == "Complete")
                {
                    order.DeliveredDate = DateTime.Now;

                    var orderItems = _context.OrderItems
                        .Where(oi => oi.OrderId == orderId)
                        .ToList();

                    foreach (var item in orderItems)
                    {
                        var trending = _context.Trendings.FirstOrDefault(t => t.P_Id == item.P_Id);
                        if (trending != null)
                        {
                            trending.OrderCount += 1;
                        }
                        else
                        {
                            _context.Trendings.Add(new Trending
                            {
                                P_Id = item.P_Id,
                                OrderCount = 1,
                                TotalViews = 0,
                                TrendingScore = 0
                            });
                        }
                    }
                }

                order.Status = status;
                TempData["SuccessMessage"] = "Successfully updated the order!";
                _context.SaveChanges();
            }

            return RedirectToAction("adminOrderDetail", new { id = orderId });
        }


        // ==============================
        //    Admin cancel Order Deital
        // =============================
        [HttpPost]
        public async Task<IActionResult> AdminCancelOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = "Cancelled";
            order.CancelRequested = false; 
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Successfully Cancelled the order!";
            return RedirectToAction("adminOrderDetail", new { id = orderId });
        }

        [Route("order/khalti-success")]
        [HttpGet]
        public async Task<IActionResult> KhaltiSuccess(
        string purchase_order_id,
        string status,
        long total_amount,  // amount is usually in paisa (int or long)
        string mobile,
        string address = null) // address might come from elsewhere (e.g. session or user input)
        {
            if (status != "Completed")
            {
                TempData["Error"] = "Payment not completed.";
                return RedirectToAction("Cart", "Shop");
            }

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            var cartItems = _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Cart", "Shop");
            }

            decimal totalInRupees = total_amount / 100m; 

            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalInRupees,
                ShippingAddress = user?.Address,
                PaymentMethod = "Khalti",
                OrderDate = DateTime.Now,
                Status = "Pending"
 
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    P_Id = item.P_Id,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                });
            }

            await _context.SaveChangesAsync();
            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
        }

    }
}
