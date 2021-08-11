using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request
{
    public readonly struct TokenIssuanceParams
    {
        [JsonPropertyName("billing_account_id")]
        public string BillingAccountId { get; init; }
        
        [JsonPropertyName("token_details")]
        public TokenDetails TokenDetails { get; init; }
        
        [JsonPropertyName("reconciliation_fields")]
        public ReconciliationFields ReconciliationFields { get; init; }
    }
}