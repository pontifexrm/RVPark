using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RVPark.Data
{
    public class SmsRequest
    {
        [Required]
        [JsonPropertyName("to")]
        public string To { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("from")]
        public string? From { get; set; }

        [JsonPropertyName("schedule")]
        public DateTime? Schedule { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }
    }

    public class SmsResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("messageId")]
        public string? MessageId { get; set; }

        [JsonPropertyName("remainingCredits")]
        public int? RemainingCredits { get; set; }
    }

    public class SmsStatus
    {
        [JsonPropertyName("messageId")]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("deliveredAt")]
        public DateTime? DeliveredAt { get; set; }
    }

    public class SmsBalance
    {
        [JsonPropertyName("balance")]
        public decimal Balance { get; set; }

        [JsonPropertyName("credits")]
        public int Credits { get; set; }
    }

}
