using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request;

public readonly struct TokenIdentifier
{
    [JsonPropertyName("token_number")]
    public string TokenNumber { get; init; }
}