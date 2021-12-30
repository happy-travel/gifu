using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.Ixaris.Response;

public readonly struct Envelope
{
    [JsonPropertyName("statusCode")]
    public string StatusCode { get; init; }

    [JsonPropertyName("httpStatusCode")]
    public int HttpStatusCode { get; init; }
}