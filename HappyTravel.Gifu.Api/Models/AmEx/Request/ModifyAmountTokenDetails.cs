using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request
{
    public readonly struct ModifyAmountTokenDetails
    {
        [JsonPropertyName("token_amount")]
        public string TokenAmount { get; init; }
    }
}