using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.Ixaris.Response
{
    public readonly struct VccDetails
    {
        [JsonPropertyName("cardholderName")]
        public string CardholderName { get; init; }

        [JsonPropertyName("cardNumber")]
        public string CardNumber { get; init; }

        [JsonPropertyName("cardReference")]
        public string CardReference { get; init; }

        [JsonPropertyName("cvv")]
        public string Cvv { get; init; }

        [JsonPropertyName("expiryDateMonth")]
        public string ExpiryDateMonth { get; init; }

        [JsonPropertyName("expiryDateYear")]
        public string ExpiryDateYear { get; init; }

        [JsonPropertyName("startDateMonth")]
        public string StartDateMonth { get; init; }

        [JsonPropertyName("startDateYear")]
        public string StartDateYear { get; init; }
    }
}
