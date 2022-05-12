using System;

namespace HappyTravel.Gifu.Api.Services.CurrencyConverter;

public static class CurrencyConverterConstants
{
    public const string CurrencyConverterClient = "CurrencyConverterClient";

    public static readonly TimeSpan RequestCacheLifeTime = TimeSpan.FromHours(1);
}
