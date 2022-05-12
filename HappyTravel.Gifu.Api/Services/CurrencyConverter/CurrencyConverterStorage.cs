using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Money.Enums;
using System;
using System.Threading.Tasks;

namespace HappyTravel.Gifu.Api.Services.CurrencyConverter;

public class CurrencyConverterStorage
{
    public CurrencyConverterStorage(IDoubleFlow flow)
    {
        _flow = flow;
    }


    public async Task<decimal?> Get(Currencies sourceCurrency, Currencies targetCurrency)
    {
        var currencyRate = await _flow.GetAsync<decimal?>(BuildKey(sourceCurrency, targetCurrency), RequestCacheLifeTime);

        return currencyRate;
    }


    public Task Set(Currencies sourceCurrency, Currencies targetCurrency, decimal currencyRate)
        => _flow.SetAsync(BuildKey(sourceCurrency, targetCurrency), currencyRate, RequestCacheLifeTime);


    private string BuildKey(Currencies sourceCurrency, Currencies targetCurrency)
        => _flow.BuildKey(nameof(CurrencyConverterStorage), sourceCurrency.ToString(), targetCurrency.ToString());


    private static TimeSpan RequestCacheLifeTime => CurrencyConverterConstants.RequestCacheLifeTime;

    private readonly IDoubleFlow _flow;
}
