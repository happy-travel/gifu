using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request
{
    public readonly struct DeleteRequest
    {
        [JsonPropertyName("token_reference_id")]
        public string TokenReferenceId { get; init; }
    }
}