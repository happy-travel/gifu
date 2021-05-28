using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Response
{
    public readonly struct TokenIssuanceData
    {
        [JsonPropertyName("token_details")]
        public TokenDetails TokenDetails { get; init; }
    }
}