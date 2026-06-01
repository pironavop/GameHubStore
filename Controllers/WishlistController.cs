using GameHubStore.Data;
using GameHubStore.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameHubStore.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var wishlist = await _context.Wishlists
                .Include(w => w.Game)
                    .ThenInclude(g => g.Category)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(wishlist);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int gameId)
        {
            var userId = _userManager.GetUserId(User);

            var exists = await _context.Wishlists
                .AnyAsync(w => w.UserId == userId && w.GameId == gameId);

            //if (!exists)
            //{
            //    _context.Wishlists.Add(new Wishlist
            //    {
            //        UserId = userId!,
            //        GameId = gameId
            //    });

            //    await _context.SaveChangesAsync();
            //}

            if (!exists)
            {
                _context.Wishlists.Add(new Wishlist
                {
                    UserId = userId!,
                    GameId = gameId
                });

                await _context.SaveChangesAsync();

                TempData["Success"] = "Game added to wishlist.";
            }
            else
            {
                TempData["Info"] = "Game already exists in wishlist.";
            }

            return RedirectToAction("Details", "Store", new { id = gameId });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var wishlistItem = await _context.Wishlists.FindAsync(id);

            if (wishlistItem != null)
            {
                _context.Wishlists.Remove(wishlistItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MoveToCart(int wishlistId)
        {
            var userId = _userManager.GetUserId(User);

            var wishlistItem = await _context.Wishlists
                .Include(w => w.Game)
                .FirstOrDefaultAsync(w =>
                    w.Id == wishlistId &&
                    w.UserId == userId);

            if (wishlistItem == null)
                return RedirectToAction(nameof(Index));

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId!
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.CartItems
                .FirstOrDefault(x => x.GameId == wishlistItem.GameId);

            if (existingItem == null)
            {
                cart.CartItems.Add(new CartItem
                {
                    GameId = wishlistItem.GameId,
                    Quantity = 1,
                    UnitPrice = wishlistItem.Game.Price
                });
            }

            _context.Wishlists.Remove(wishlistItem);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Game moved to cart.";

            return RedirectToAction(nameof(Index));
        }
    }
}