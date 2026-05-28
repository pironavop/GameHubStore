namespace GameHubStore.Models.ViewModels
{
    public class RazorpayCheckoutViewModel
    {
        public int OrderId { get; set; }
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpayKeyId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;   
    }
}
