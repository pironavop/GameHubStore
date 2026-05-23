using GameHubStore.Data;
using GameHubStore.Models.Entities;
using GameHubStore.Models.Enums;
using GameHubStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameHubStore.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Game)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                Cart = cart,
                TotalAmount = cart.CartItems.Sum(x => x.UnitPrice * x.Quantity)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = _userManager.GetUserId(User);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Game)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            foreach (var item in cart.CartItems)
            {
                if (item.Game.StockQuantity < item.Quantity)
                {
                    TempData["Error"] = $"{item.Game.Title} is out of stock.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            var order = new Order
            {
                UserId = userId!,
                OrderDate = DateTime.UtcNow,
                TotalAmount = cart.CartItems.Sum(x => x.UnitPrice * x.Quantity),
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending
            };

            foreach (var item in cart.CartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    GameId = item.GameId,
                    Quantity = item.Quantity,
                    Price = item.UnitPrice
                });

                item.Game.StockQuantity -= item.Quantity;
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Order created successfully. Please complete payment.";

            return RedirectToAction("Details", new { id = order.Id });
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Game)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Game)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            return View(order);
        }
    }
}