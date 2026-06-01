using GameHubStore.Data;
using GameHubStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameHubStore.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Add(int gameId, int rating, string comment)
        {
            var userId = _userManager.GetUserId(User);

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Rating must be between 1 and 5.";
                return RedirectToAction("Details", "Store", new { id = gameId });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Review comment is required.";
                return RedirectToAction("Details", "Store", new { id = gameId });
            }

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.GameId == gameId);

            if (alreadyReviewed)
            {
                TempData["Info"] = "You have already reviewed this game.";
                return RedirectToAction("Details", "Store", new { id = gameId });
            }

            var review = new Review
            {
                UserId = userId!,
                GameId = gameId,
                Rating = rating,
                Comment = comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Review added successfully.";
            return RedirectToAction("Details", "Store", new { id = gameId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int gameId)
        {
            var userId = _userManager.GetUserId(User);

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Review deleted.";
            }

            return RedirectToAction("Details", "Store", new { id = gameId });
        }
    }
}