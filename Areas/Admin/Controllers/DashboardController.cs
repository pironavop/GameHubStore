using GameHubStore.Data;
using GameHubStore.Models.Enums;
using GameHubStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameHubStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalGames = await _context.Games.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalPlatforms = await _context.Platforms.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),

                PendingOrders = await _context.Orders
                    .CountAsync(o => o.OrderStatus == OrderStatus.Pending),

                PaidOrders = await _context.Orders
                    .CountAsync(o => o.PaymentStatus == PaymentStatus.Paid),

                PendingPayments = await _context.Orders
                    .CountAsync(o => o.PaymentStatus == PaymentStatus.Pending),

                TotalRevenue = await _context.Orders
                    .Where(o => o.PaymentStatus == PaymentStatus.Paid)
                    .SumAsync(o => o.TotalAmount),

                AvailableKeys = await _context.GameKeys
                    .CountAsync(k => !k.IsSold),

                SoldKeys = await _context.GameKeys
                    .CountAsync(k => k.IsSold)
            };

            return View(model);
        }
    }
}