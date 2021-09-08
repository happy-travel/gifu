using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request
{
    public readonly struct ModifyRequest
    {
        [JsonPropertyName("token_issuance_params")]
        public ModifyAmountTokenIssuanceParams TokenIssuanceParams { get; init; }
    }
}