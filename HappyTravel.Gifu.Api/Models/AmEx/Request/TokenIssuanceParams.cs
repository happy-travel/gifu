using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request
{
    public readonly struct TokenIssuanceParams
    {
        [JsonPropertyName("token_details")]
        public TokenDetails TokenDetails { get; init; }
    }
}