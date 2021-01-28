using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiChat.Web.Auth.Models
{
    public class PaddleResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("response")]
        public List<PaddleWebhookResponse> Response { get; set; }
    }

    public class PaddleWebhookResponse
    {
        [JsonPropertyName("subscription_id")]
        public int subscription_id { get; set; }
        [JsonPropertyName("plan_id")]
        public int plan_id { get; set; }
        [JsonPropertyName("user_id")]
        public int user_id { get; set; }
        [JsonPropertyName("user_email")]
        public string user_email { get; set; }
        [JsonPropertyName("marketing_consent")]
        public bool marketing_consent { get; set; }
        [JsonPropertyName("update_url")]
        public string update_url { get; set; }
        [JsonPropertyName("cancel_url")]
        public string cancel_url { get; set; }
        [JsonPropertyName("state")]
        public string state { get; set; }
        [JsonPropertyName("signup_date")]
        public string signup_date { get; set; }
        [JsonPropertyName("quantity")]
        public int quantity { get; set; }
        [JsonPropertyName("last_payment")]
        public Payment last_payment { get; set; }
        [JsonPropertyName("payment_information")]
        public PaymentInformation payment_information { get; set; }
        [JsonPropertyName("next_payment")]
        public Payment next_payment { get; set; }
    }

    public class PaymentInformation
    {
        [JsonPropertyName("payment_method")]
        public string payment_method { get; set; }
        [JsonPropertyName("card_type")]
        public string card_type { get; set; }
        [JsonPropertyName("last_four_digits")]
        public string last_four_digits { get; set; }
        [JsonPropertyName("expiry_date")]
        public string expiry_date { get; set; }
    }

    public class Payment
    {
        [JsonPropertyName("amount")]
        public int amount { get; set; }
        [JsonPropertyName("currency")]
        public string currency { get; set; }
        [JsonPropertyName("date")]
        public string date { get; set; }
    }
}
