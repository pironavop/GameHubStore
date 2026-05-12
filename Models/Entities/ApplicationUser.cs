using Microsoft.AspNetCore.Identity;

namespace GameHubStore.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<GameKey> PurchasedKeys { get; set; } = new List<GameKey>();

    }
}
