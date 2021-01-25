using System.Text.Json.Serialization;

namespace ApiChat.Web.Auth.Models
{
    public class PaddleResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("response")]
        public PaddleWebhookResponse Response { get; set; }
    }

    public class PaddleWebhookResponse
    {
        [JsonPropertyName("subscription_id")]
        public int SubscribtionId { get; set; }
        [JsonPropertyName("plan_id")]
        public int PlanId { get; set; }
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }
        [JsonPropertyName("user_email")]
        public string UserEmail { get; set; }
        [JsonPropertyName("marketing_consent")]
        public bool MarketingConsent { get; set; }
        [JsonPropertyName("update_url")]
        public string UpdateUrl { get; set; }
        [JsonPropertyName("cancel_url")]
        public string CancelUrl { get; set; }
        [JsonPropertyName("state")]
        public string State { get; set; }
        [JsonPropertyName("signup_date")]
        public string SignUpDate { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("last_payment")]
        public Payment LastPayment { get; set; }
        [JsonPropertyName("payment_information")]
        public PaymentInformation PaymentInformation { get; set; }
        [JsonPropertyName("next_payment")]
        public Payment NextPayment { get; set; }
    }

    public class PaymentInformation
    {
        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; }
        [JsonPropertyName("card_type")]
        public string CardType { get; set; }
        [JsonPropertyName("last_four_digits")]
        public string LastFourDigits { get; set; }
        [JsonPropertyName("expiry_date")]
        public string ExpiryDate { get; set; }
    }

    public class Payment
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; }
        [JsonPropertyName("date")]
        public string Date { get; set; }
    }
}
