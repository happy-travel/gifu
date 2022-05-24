using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.Ixaris.Response;

public class LoginData
{
    [JsonPropertyName("userReference")]
    public string UserReference { get; init; }

    [JsonPropertyName("securityToken")]
    public string SecurityToken { get; init; }

    [JsonPropertyName("dormant")]
    public string Dormant { get; init; }
}