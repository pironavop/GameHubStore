using GameHubStore.Data;
using GameHubStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GameHubStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GameKeysController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GameKeysController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var keys = await _context.GameKeys
                .Include(k => k.Game)
                .Include(k => k.SoldToUser)
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();

            return View(keys);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Games = await GetGamesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int gameId, string keyCode)
        {
            if (gameId <= 0)
                ModelState.AddModelError("", "Please select a game.");

            if (string.IsNullOrWhiteSpace(keyCode))
                ModelState.AddModelError("", "Key code is required.");

            var exists = await _context.GameKeys.AnyAsync(k => k.KeyCode == keyCode);

            if (exists)
                ModelState.AddModelError("", "This key already exists.");

            if (!ModelState.IsValid)
            {
                ViewBag.Games = await GetGamesAsync();
                return View();
            }

            var gameKey = new GameKey
            {
                GameId = gameId,
                KeyCode = keyCode.Trim(),
                IsSold = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.GameKeys.Add(gameKey);

            var game = await _context.Games.FindAsync(gameId);

            if (game != null)
            {
                game.StockQuantity += 1;
                game.IsActive = true;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Game key added successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetGamesAsync()
        {
            return await _context.Games
                .OrderBy(g => g.Title)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Title
                })
                .ToListAsync();
        }
    }
}