namespace GameHubStore.Models.Entities
{
    public class Platform
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;


        public ICollection<GamePlatform> GamePlatforms { get; set; } = new List<GamePlatform>();
    }
}
