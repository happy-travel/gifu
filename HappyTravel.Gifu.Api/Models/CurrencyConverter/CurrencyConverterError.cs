using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.CurrencyConverter;

public readonly struct CurrencyConverterError
{
    [JsonPropertyName("errors")]
    public CurrencyConverterErrorDetails Errors { get; init; }
}
