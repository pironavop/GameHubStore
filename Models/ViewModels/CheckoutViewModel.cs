using GameHubStore.Models.Entities;

namespace GameHubStore.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public Cart Cart { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }
}