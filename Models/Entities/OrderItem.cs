namespace GameHubStore.Models.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int GameId { get; set; }
        public Game Game { get; set; } = null!;

        public int Quantity { get; set; } = 1;
        public decimal Price { get; set; }
    }
}