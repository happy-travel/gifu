using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Response;

public readonly struct AmexResponse
{
    [JsonPropertyName("status")]
    public Status Status { get; init; }
        
    [JsonPropertyName("token_issuance_data")]
    public TokenIssuanceData TokenIssuanceData { get; init; }
}