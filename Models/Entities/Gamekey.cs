namespace GameHubStore.Models.Entities
{
    public class GameKey
    {
        public int Id { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; } = null!;

        public string KeyCode { get; set; } = string.Empty;
        public bool IsSold { get; set; } = false;

        public string? SoldToUserId { get; set; }
        public ApplicationUser? SoldToUser { get; set; }

        public int? SoldOrderId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}