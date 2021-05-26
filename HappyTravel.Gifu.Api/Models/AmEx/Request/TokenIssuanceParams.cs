using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request
{
    public struct TokenIssuanceParams
    {
        [JsonPropertyName("token_details")]
        public TokenDetails TokenDetails { get; init; }
    }
}