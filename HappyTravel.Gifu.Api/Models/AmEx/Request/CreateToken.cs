using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request
{
    public readonly struct CreateToken
    {
        [JsonPropertyName("token_issuance_params")]
        public TokenIssuanceParams TokenIssuanceParams { get; init; }
    }
}