using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Response
{
    public readonly struct Status
    {
        [JsonPropertyName("short_message")]
        public string ShortMessage { get; init; }
        
        [JsonPropertyName("detailed_message")]
        public string DetailedMessage { get; init; }
    }
}