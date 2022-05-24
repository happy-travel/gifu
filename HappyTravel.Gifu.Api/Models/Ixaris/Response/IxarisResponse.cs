using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.Ixaris.Response;

public class IxarisResponse<T>
{
    [JsonPropertyName("details")]
    public Dictionary<string, string> Details { get; init; }

    [JsonPropertyName("body")]
    public T Body { get; init; }
}