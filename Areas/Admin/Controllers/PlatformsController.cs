using GameHubStore.Data;
using GameHubStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameHubStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PlatformsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlatformsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var platforms = await _context.Platforms
                .OrderByDescending(p => p.CreateAt)
                .ToListAsync();

            return View(platforms);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Platform platform)
        {
            if (!ModelState.IsValid)
                return View(platform);

            platform.CreateAt = DateTime.UtcNow;

            _context.Platforms.Add(platform);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var platform = await _context.Platforms.FindAsync(id);

            if (platform == null)
                return NotFound();

            return View(platform);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Platform platform)
        {
            if (!ModelState.IsValid)
                return View(platform);

            var existingPlatform = await _context.Platforms.FindAsync(platform.Id);

            if (existingPlatform == null)
                return NotFound();

            existingPlatform.Name = platform.Name;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var platform = await _context.Platforms.FindAsync(id);

            if (platform == null)
                return NotFound();

            return View(platform);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var platform = await _context.Platforms
                .Include(p => p.GamePlatforms)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (platform == null)
                return NotFound();

            if (platform.GamePlatforms.Any())
            {
                ModelState.AddModelError("", "This platform cannot be deleted because games are linked to it.");
                return View(platform);
            }

            _context.Platforms.Remove(platform);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}