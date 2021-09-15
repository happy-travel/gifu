using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request
{
    public readonly struct ModifyAmountTokenIssuanceParams
    {
        [JsonPropertyName("billing_account_id")]
        public string BillingAccountId { get; init; }
        
        [JsonPropertyName("token_details")]
        public ModifyAmountTokenDetails TokenDetails { get; init; }
    }
}