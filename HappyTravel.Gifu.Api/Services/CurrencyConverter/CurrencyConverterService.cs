using CSharpFunctionalExtensions;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using System;
using System.Threading.Tasks;

namespace HappyTravel.Gifu.Api.Services.CurrencyConverter;

public class CurrencyConverterService
{
    public CurrencyConverterService(CurrencyConverterClient currencyConverterClient,
        CurrencyConverterStorage currencyConverterStorage)
    {
        _currencyConverterClient = currencyConverterClient;
        _currencyConverterStorage = currencyConverterStorage;
    }


    public async Task<Result<MoneyAmount>> ConvertToCurrency(MoneyAmount moneyAmount, Currencies targetCurrency)
    {
        var sourceCurrency = moneyAmount.Currency;

        return await GetOrSetRate()
            .Map((currencyRate) => new MoneyAmount(Math.Round(currencyRate * moneyAmount.Amount, 2), targetCurrency));


        async Task<Result<decimal>> GetOrSetRate()
        {
            var cachedCurrencyRate = await _currencyConverterStorage.Get(sourceCurrency, targetCurrency);

            if (cachedCurrencyRate is not null)
                return cachedCurrencyRate.Value;

            return await _currencyConverterClient.GetRate(sourceCurrency, targetCurrency)
                .Tap((currencyRate) => _currencyConverterStorage.Set(sourceCurrency, targetCurrency, currencyRate));
        }
    }


    private readonly CurrencyConverterClient _currencyConverterClient;
    private readonly CurrencyConverterStorage _currencyConverterStorage;
}
