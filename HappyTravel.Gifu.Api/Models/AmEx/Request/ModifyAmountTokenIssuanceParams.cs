using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request
{
    public readonly struct ModifyAmountTokenIssuanceParams
    {
        [JsonPropertyName("token_details")]
        public ModifyAmountTokenDetails TokenDetails { get; init; }
    }
}