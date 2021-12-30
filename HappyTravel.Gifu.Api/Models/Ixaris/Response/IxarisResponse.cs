using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.Ixaris.Response;

public readonly struct IxarisResponse<T>
{
    [JsonPropertyName("details")]
    public object Details { get; init; } // The type is unknown.

    [JsonPropertyName("body")]
    public T Body { get; init; }
}