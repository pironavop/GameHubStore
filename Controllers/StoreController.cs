using GameHubStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameHubStore.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StoreController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var games = await _context.Games
                .Include(g => g.Category)
                .Include(g => g.GamePlatforms)
                    .ThenInclude(gp => gp.Platform)
                    .Include(g => g.Reviews)
                .Where(g => g.IsActive)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return View(games);
        }

        public async Task<IActionResult> Details(int id)
        {
            //var game = await _context.Games
            //    .Include(g => g.Category)
            //    .Include(g => g.GamePlatforms)
            //        .ThenInclude(gp => gp.Platform)
            //    .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);

            var game = await _context.Games
                    .Include(g => g.Category)
                    .Include(g => g.GamePlatforms)
                        .ThenInclude(gp => gp.Platform)
                    .Include(g => g.Reviews)
                        .ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);

            if (game == null)
                return NotFound();

            return View(game);
        }
    }
}