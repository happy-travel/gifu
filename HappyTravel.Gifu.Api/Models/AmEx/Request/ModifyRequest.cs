using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request;

public readonly struct ModifyRequest
{
    [JsonPropertyName("token_identifier")]
    public TokenIdentifier TokenIdentifier { get; init; }
        
    [JsonPropertyName("token_issuance_params")]
    public TokenIssuanceParams TokenIssuanceParams { get; init; }
}