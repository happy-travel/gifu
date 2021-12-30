using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request;

public readonly struct CustomField
{
    [JsonPropertyName("index")]
    public int Index { get; init; }
        
    [JsonPropertyName("value")]
    public string Value { get; init; }
}