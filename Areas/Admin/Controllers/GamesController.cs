using GameHubStore.Data;
using GameHubStore.Models.Entities;
using GameHubStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GameHubStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        //public GamesController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        public GamesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var games = await _context.Games
                .Include(g => g.Category)
                .Include(g => g.GamePlatforms)
                    .ThenInclude(gp => gp.Platform)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return View(games);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new GameViewModel
            {
                ReleaseDate = DateTime.Today,
                Categories = await GetCategoriesAsync(),
                Platforms = await GetPlatformsAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(GameViewModel model)
        {
            if (!model.SelectedPlatformIds.Any())
            {
                ModelState.AddModelError("SelectedPlatformIds", "Please select at least one platform.");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategoriesAsync();
                model.Platforms = await GetPlatformsAsync();
                return View(model);
            }

            string? coverImagePath = null;

            try
            {
                coverImagePath = await UploadCoverImageAsync(model.CoverImageFile);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("CoverImageFile", ex.Message);
                model.Categories = await GetCategoriesAsync();
                model.Platforms = await GetPlatformsAsync();
                return View(model);
            }

            var game = new Game
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                DiscountPrice = model.DiscountPrice,
                ReleaseDate = model.ReleaseDate,
                CoverImageUrl = coverImagePath,
                TrailerUrl = model.TrailerUrl,
                StockQuantity = model.StockQuantity,
                IsActive = model.IsActive,
                CategoryId = model.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var platformId in model.SelectedPlatformIds)
            {
                game.GamePlatforms.Add(new GamePlatform
                {
                    PlatformId = platformId
                });
            }

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var game = await _context.Games
                .Include(g => g.GamePlatforms)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
                return NotFound();

            var model = new GameViewModel
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                Price = game.Price,
                DiscountPrice = game.DiscountPrice,
                ReleaseDate = game.ReleaseDate,
                CoverImageUrl = game.CoverImageUrl,
                TrailerUrl = game.TrailerUrl,
                StockQuantity = game.StockQuantity,
                IsActive = game.IsActive,
                CategoryId = game.CategoryId,
                SelectedPlatformIds = game.GamePlatforms.Select(gp => gp.PlatformId).ToList(),
                Categories = await GetCategoriesAsync(),
                Platforms = await GetPlatformsAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(GameViewModel model)
        {
            if (!model.SelectedPlatformIds.Any())
            {
                ModelState.AddModelError("SelectedPlatformIds", "Please select at least one platform.");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategoriesAsync();
                model.Platforms = await GetPlatformsAsync();
                return View(model);
            }

            var game = await _context.Games
                .Include(g => g.GamePlatforms)
                .FirstOrDefaultAsync(g => g.Id == model.Id);

            if (game == null)
                return NotFound();

            if (model.CoverImageFile != null)
            {
                try
                {
                    var newImagePath = await UploadCoverImageAsync(model.CoverImageFile);

                    if (!string.IsNullOrEmpty(game.CoverImageUrl))
                    {
                        var oldImagePath = Path.Combine(
                            _webHostEnvironment.WebRootPath,
                            game.CoverImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                        );

                        if (System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);
                    }

                    game.CoverImageUrl = newImagePath;
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("CoverImageFile", ex.Message);
                    model.Categories = await GetCategoriesAsync();
                    model.Platforms = await GetPlatformsAsync();
                    return View(model);
                }
            }

            game.Title = model.Title;
            game.Description = model.Description;
            game.Price = model.Price;
            game.DiscountPrice = model.DiscountPrice;
            game.ReleaseDate = model.ReleaseDate;
            //game.CoverImageUrl = model.CoverImageUrl;
            game.TrailerUrl = model.TrailerUrl;
            game.StockQuantity = model.StockQuantity;
            game.IsActive = model.IsActive;
            game.CategoryId = model.CategoryId;

            game.GamePlatforms.Clear();

            foreach (var platformId in model.SelectedPlatformIds)
            {
                game.GamePlatforms.Add(new GamePlatform
                {
                    GameId = game.Id,
                    PlatformId = platformId
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var game = await _context.Games
                .Include(g => g.Category)
                .Include(g => g.GamePlatforms)
                    .ThenInclude(gp => gp.Platform)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
                return NotFound();

            return View(game);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Games
                .Include(g => g.GamePlatforms)
                .Include(g => g.OrderItems)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
                return NotFound();

            if (game.OrderItems.Any())
            {
                ModelState.AddModelError("", "This game cannot be deleted because it exists in orders.");
                return View(game);
            }

            if (!string.IsNullOrEmpty(game.CoverImageUrl))
            {
                var imagePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    game.CoverImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.GamePlatforms.RemoveRange(game.GamePlatforms);
            _context.Games.Remove(game);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetPlatformsAsync()
        {
            return await _context.Platforms
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToListAsync();
        }

        private async Task<string?> UploadCoverImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Only JPG, JPEG, PNG, and WEBP images are allowed.");

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "games");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/games/{uniqueFileName}";
        }
    }
}

