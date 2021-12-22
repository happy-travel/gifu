using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.Ixaris.Response
{
    public readonly struct LoginData
    {
        [JsonPropertyName("userReference")]
        public readonly string UserReference { get; init; }

        [JsonPropertyName("securityToken")]
        public readonly string SecurityToken { get; init; }

        [JsonPropertyName("dormant")]
        public readonly bool Dormant { get; init; }
    }
}
