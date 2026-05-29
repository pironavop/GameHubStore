using GameHubStore.Data;
using GameHubStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameHubStore.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LibraryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var keys = await _context.GameKeys
                .Include(k => k.Game)
                    .ThenInclude(g => g.Category)
                .Where(k => k.SoldToUserId == userId && k.IsSold)
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();

            return View(keys);
        }
    }
}