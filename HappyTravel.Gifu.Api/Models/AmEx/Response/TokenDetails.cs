using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Response;

public readonly struct TokenDetails
{
    [JsonPropertyName("token_number")]
    public string TokenNumber { get; init; }
        
    [JsonPropertyName("token_expiry_date")]
    public string TokenExpiryDate { get; init; }
        
    [JsonPropertyName("token_security_code")]
    public string TokenSecurityCode { get; init; }
}