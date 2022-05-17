using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.CurrencyConverter;

public readonly struct CurrencyConverterErrorDetails
{
    [JsonPropertyName("targetCurrency")]
    public List<string> TargetCurrency { get; init; }
}
