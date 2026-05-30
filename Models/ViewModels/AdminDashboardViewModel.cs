namespace GameHubStore.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalGames { get; set; }
        public int TotalCategories { get; set; }
        public int TotalPlatforms { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PaidOrders { get; set; }
        public int PendingPayments { get; set; }
        public decimal TotalRevenue { get; set; }
        public int AvailableKeys { get; set; }
        public int SoldKeys { get; set; }
    }
}