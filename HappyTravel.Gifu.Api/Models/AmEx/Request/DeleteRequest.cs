using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request;

public readonly struct DeleteRequest
{
    [JsonPropertyName("token_reference_id")]
    public string TokenReferenceId { get; init; }
        
    [JsonPropertyName("billing_account_id")]
    public string BillingAccountId { get; init; }
}