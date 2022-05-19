using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.Ixaris.Response;

public class BaseIxarisResponse<T>
{
    [JsonPropertyName("envelope")]
    public Envelope Envelope { get; init; }

    [JsonPropertyName("response")]
    public IxarisResponse<T> Response { get; init; }
}