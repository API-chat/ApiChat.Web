using Microsoft.AspNetCore.Http;

namespace ApiChat.Web.Auth.Controllers
{
    public class PaddleWebhookRequest
    {
        public PaddleWebhookRequest(IFormCollection form)
        {
            if (form.TryGetValue("alert_name", out var alertName))
            {
                AlertName = alertName;
            }
            if (form.TryGetValue("alert_id", out var alertId))
            {
                AlertId = alertId;
            }
            if (form.TryGetValue("checkout_id", out var checkoutId))
            {
                CheckoutId = checkoutId;
            }
            if (form.TryGetValue("passthrough", out var passthrough))
            {
                Passthrough = passthrough;
            }
            if (form.TryGetValue("status", out var status))
            {
                Status = status;
            }
            if (form.TryGetValue("subscription_id", out var subscriptionId))
            {
                SubscriptionId = subscriptionId;
            }
            if (form.TryGetValue("subscription_plan_id", out var subscriptionPlanId))
            {
                SubscriptionPlanId = subscriptionPlanId;
            }
            if (form.TryGetValue("user_id", out var userId))
            {
                UserId = userId;
            }
            if (form.TryGetValue("p_signature", out var pSignature))
            {
                PSignature = pSignature;
            }
            if (form.TryGetValue("old_subscription_plan_id", out var oldSubscriptionPlanId))
            {
                OldSubscriptionPlanId = oldSubscriptionPlanId;
            }
        }
        public string AlertName { get; set; }
        public string AlertId { get; set; }
        public string CheckoutId { get; set; }
        public string Passthrough { get; set; }
        public string Status { get; set; }
        public string SubscriptionId { get; set; }
        public string UserId { get; set; }
        public string PSignature { get; set; }
        public string SubscriptionPlanId { get; set; }
        public string OldSubscriptionPlanId { get; set; }
    }
}
