using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request
{
    public readonly struct TokenDetails
    {
        [JsonPropertyName("token_reference_id")]
        public string TokenReferenceId { get; init; }
        
        [JsonPropertyName("token_amount")]
        public string TokenAmount { get; init; }
        
        [JsonPropertyName("token_start_date")]
        public string TokenStartDate { get; init; }
        
        [JsonPropertyName("token_end_date")]
        public string TokenEndDate { get; init; }
    }
}