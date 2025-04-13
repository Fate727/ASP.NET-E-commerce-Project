using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Techhive.Data; // Replace with your actual DbContext namespace
using Techhive.Models;
using Techhive.ProductModels;
using Techhive.OrderModels;

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
        public IActionResult Checkout(CheckoutViewModel model)
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

            // Create a new order
            var order = new Order
            {
                UserId = userId,
                TotalAmount = total,
                ShippingAddress = $"{model.Address}",
                PaymentMethod = model.PaymentMethod,
                OrderDate = DateTime.Now,
                Status = "Pending"
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Add order items
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,  // Link the order
                    P_Id = cartItem.P_Id,     // Product ID
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price
                };
                _context.OrderItems.Add(orderItem);
            }

            _context.SaveChanges();

            _context.Carts.RemoveRange(cartItems);
            _context.SaveChanges();

            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
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


        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order != null)
            {
                TempData["SuccessMessage"] = "Succesfully Update the Order!";
                // Update the order status
                order.Status = status;
                _context.SaveChanges();
            }

            return RedirectToAction("adminOrderDetail", new { id = orderId });
        }

    }
}
