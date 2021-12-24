using HappyTravel.Money.Enums;
using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.Ixaris.Request
{
    public readonly struct IssueVccRequest
    {
        [JsonPropertyName("currency")]
        public Currencies? Currency { get; init; }
        
        [JsonPropertyName("fundingAccountReference")]
        public string? FundingAccountReference { get; init; }

        [JsonPropertyName("amount")]
        public decimal? Amount { get; init; }     
    }
}
