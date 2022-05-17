using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure;
using HappyTravel.Gifu.Api.Models.CurrencyConverter;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HappyTravel.Gifu.Api.Services.CurrencyConverter;

public class CurrencyConverterClient
{
    public CurrencyConverterClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContext = httpContextAccessor.HttpContext;
    }


    public Task<Result<decimal>> GetRate(Currencies sourceCurrency, Currencies targetCurrency)
    {
        var url = $"api/1.0/Rates/{sourceCurrency}/{targetCurrency}";

        return Get<decimal>(new Uri(url, UriKind.Relative));
    }


    private Task<Result<TResponse>> Get<TResponse>(Uri url)
        => Send<TResponse>(new HttpRequestMessage(HttpMethod.Get, url));


    private async Task<Result<TResponse>> Send<TResponse>(HttpRequestMessage request)
    {
        var client = _httpClientFactory.CreateClient(HttpClientNames.CurrencyConverterClient);
        using var response = await client.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return JsonSerializer.Deserialize<TResponse>(content);

        try
        {
            var responseError = JsonSerializer.Deserialize<CurrencyConverterError>(content);

            return Result.Failure<TResponse>(string.Join("; ", responseError.Errors.TargetCurrency));
        }
        catch (JsonException ex)
        {
            return Result.Failure<TResponse>("Server error");
        }
    }


    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpContext _httpContext;
}
