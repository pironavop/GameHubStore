using GameHubStore.Data;
using GameHubStore.Models.Entities;
using GameHubStore.Models.Enums;
using GameHubStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;

namespace GameHubStore.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public PaymentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Pay(int orderId)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                return NotFound();

            if (order.PaymentStatus == PaymentStatus.Paid)
                return RedirectToAction("Details", "Order", new { id = order.Id });

            var keyId = _configuration["Razorpay:KeyId"];
            var keySecret = _configuration["Razorpay:KeySecret"];

            if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
                throw new Exception("Razorpay keys are missing.");

            RazorpayClient client = new RazorpayClient(keyId, keySecret);

            //var amountInPaise = (int)(order.TotalAmount * 100);

            var amountInPaise = Convert.ToInt32(order.TotalAmount * 100);

            if (amountInPaise < 100)
            {
                TempData["Error"] = "Order amount must be at least ₹1 for online payment.";
                return RedirectToAction("Details", "Order", new { id = order.Id });
            }

            Dictionary<string, object> options = new Dictionary<string, object>
            {
                { "amount", amountInPaise },
                { "currency", "INR" },
                { "receipt", $"order_{order.Id}" },
                { "payment_capture", 1 }
            };

            Razorpay.Api.Order razorpayOrder = client.Order.Create(options);

            var razorpayOrderId = razorpayOrder["id"].ToString();

            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == order.Id);

            if (existingPayment == null)
            {
                existingPayment = new Models.Entities.Payment
                {
                    OrderId = order.Id,
                    PaymentMethod = "Razorpay",
                    TransactionId = razorpayOrderId,
                    Amount = order.TotalAmount,
                    PaymentDate = DateTime.UtcNow,
                    Status = PaymentStatus.Pending
                };

                _context.Payments.Add(existingPayment);
            }
            else
            {
                existingPayment.TransactionId = razorpayOrderId;
                existingPayment.Status = PaymentStatus.Pending;
                existingPayment.PaymentDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var model = new RazorpayCheckoutViewModel
            {
                OrderId = order.Id,
                RazorpayOrderId = razorpayOrderId,
                RazorpayKeyId = keyId,
                Amount = order.TotalAmount,
                UserName = order.User.FullName,
                UserEmail = order.User.Email ?? ""
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Verify(
            int orderId,
            string razorpay_payment_id,
            string razorpay_order_id,
            string razorpay_signature)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                return NotFound();

            var keySecret = _configuration["Razorpay:KeySecret"];

            var isValid = VerifyPaymentSignature(
                razorpay_order_id,
                razorpay_payment_id,
                razorpay_signature,
                keySecret!
            );

            if (!isValid)
            {
                order.PaymentStatus = PaymentStatus.Failed;

                if (order.Payment != null)
                    order.Payment.Status = PaymentStatus.Failed;

                await _context.SaveChangesAsync();

                TempData["Error"] = "Payment verification failed.";
                return RedirectToAction("Details", "Order", new { id = order.Id });
            }

            order.PaymentStatus = PaymentStatus.Paid;
            order.OrderStatus = OrderStatus.Completed;
            await AssignGameKeysAsync(order.Id, userId!);

            if (order.Payment != null)
            {
                order.Payment.Status = PaymentStatus.Paid;
                order.Payment.TransactionId = razorpay_payment_id;
                order.Payment.PaymentDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment successful.";
            return RedirectToAction("Details", "Order", new { id = order.Id });
        }


        private async Task AssignGameKeysAsync(int orderId, string userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return;

            foreach (var item in order.OrderItems)
            {
                for (int i = 0; i < item.Quantity; i++)
                {
                    var availableKey = await _context.GameKeys
                        .FirstOrDefaultAsync(k => k.GameId == item.GameId && !k.IsSold);

                    if (availableKey != null)
                    {
                        availableKey.IsSold = true;
                        availableKey.SoldToUserId = userId;
                        availableKey.SoldOrderId = orderId;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }


        [HttpPost]
        public async Task<IActionResult> SimulateSuccess(int orderId)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                return NotFound();

            order.PaymentStatus = PaymentStatus.Paid;
            order.OrderStatus = OrderStatus.Completed;

            if (order.Payment != null)
            {
                order.Payment.Status = PaymentStatus.Paid;
                order.Payment.TransactionId = Guid.NewGuid().ToString();
                order.Payment.PaymentDate = DateTime.UtcNow;
            }

            await AssignGameKeysAsync(order.Id, userId!);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Test payment completed successfully.";

            return RedirectToAction("Details", "Order", new { id = order.Id });
        }



        private bool VerifyPaymentSignature(
            string razorpayOrderId,
            string razorpayPaymentId,
            string razorpaySignature,
            string secret)
        {
            var payload = $"{razorpayOrderId}|{razorpayPaymentId}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return generatedSignature == razorpaySignature;
        }
    }
}