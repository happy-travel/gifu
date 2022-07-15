using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Logging;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models.Ixaris.Request;
using HappyTravel.Gifu.Api.Models.Ixaris.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HappyTravel.Gifu.Api.Services.SupplierClients;

public class IxarisClient : IIxarisClient
{
    public IxarisClient(IHttpClientFactory httpClientFactory, IOptions<IxarisOptions> options, ILogger<IxarisClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result> CancelScheduleLoad(string securityToken, string scheduleReference)
    {
        var endpoint = $"ixsol-paymentpartner/schedule/{scheduleReference}/cancel";

        var (isSuccess, _, _, error) = await Post<IxarisData>(new Uri(endpoint, UriKind.Relative), securityToken);

        return isSuccess
            ? Result.Success()
            : Result.Failure(error);
    }


    public Task<Result<VccDetails>> GetVirtualCardDetails(string securityToken, string cardReference)
    {
        var endpoint = $"ixsol-paymentpartner/virtualcards/card/{cardReference}?getCvv=true";

        return Get<VccDetails>(new Uri(endpoint, UriKind.Relative), securityToken);
    }


    public Task<Result<IssueVcc>> IssueVirtualCard(string securityToken, string virtualCardFactoryName, IssueVccRequest issueVccRequest)
    {
        var endpoint = $"ixsol-paymentpartner/virtualcards/{virtualCardFactoryName}";

        var requestContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
        {
            new("currency", issueVccRequest.Currency.ToString()),
            new("fundingAccountReference", issueVccRequest.FundingAccountReference),
            new("cardInfo", JsonSerializer.Serialize(issueVccRequest.CardInfo))
        });

        return Post<IssueVcc>(new Uri(endpoint, UriKind.Relative), requestContent, securityToken);
    }


    public async Task<Result<string>> Login()
    {
        var endpoint = $"commons/auth/login";

        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(endpoint, UriKind.Relative))
        {
            Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()  // Because in the example Content-Type: application/x-www-form-urlencoded.
            {
                new("apiKey", _options.ApiKey),
                new("password", _options.Password)
            })
        };

        var (isSuccess, _, data, error) = await SendRequest<LoginData>(request);

        return isSuccess
            ? data.SecurityToken
            : Result.Failure<string>(error);
    }


    public async Task<Result<string>> RemoveVirtualCard(string securityToken, string cardReference)
    {
        var endpoint = $"ixsol-paymentpartner/virtualcards/{cardReference}/delete";

        var (isSuccess, _, data, error) = await Post<IxarisData>(new Uri(endpoint, UriKind.Relative), securityToken);

        return isSuccess
            ? data.TransactionReference
            : Result.Failure<string>(error);
    }


    public async Task<Result<string>> ScheduleLoad(string securityToken, ScheduleLoadRequest scheduleLoadRequest)
    {
        var endpoint = $"ixsol-paymentpartner/schedule/load";

        var requestContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
        {
            new("cardReference", scheduleLoadRequest.CardReference),
            new("fundingAccountReference", scheduleLoadRequest.FundingAccountReference),
            new("amount", scheduleLoadRequest.Amount.ToString()),
            new("scheduleDate", scheduleLoadRequest.ScheduleDate),
            new("clearanceDate", scheduleLoadRequest.ClearanceDate),
        });

        var (isSuccess, _, data, error) = await Post<IxarisData>(new Uri(endpoint, UriKind.Relative), requestContent, securityToken);

        return isSuccess
            ? data.ScheduleReference
            : Result.Failure<string>(error);
    }


    public async Task<Result<string>> UpdateScheduleLoad(string securityToken, string scheduleReference, UpdateScheduleLoadRequest updateScheduleLoadRequest)
    {
        var endpoint = $"ixsol-paymentpartner/schedule/{scheduleReference}/update";

        var requestContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
        {
            new("fundingAccountReference", updateScheduleLoadRequest.FundingAccountReference),
            new("amount", updateScheduleLoadRequest.Amount.ToString()),
            new("scheduleDate", updateScheduleLoadRequest.ScheduleDate),
            new("clearanceDate", updateScheduleLoadRequest.ClearanceDate),
        });

        var (isSuccess, _, data, error) = await Post<IxarisData>(new Uri(endpoint, UriKind.Relative), requestContent, securityToken);

        return isSuccess
            ? data.ScheduleReference
            : Result.Failure<string>(error);
    }


    private Task<Result<TResponse>> Get<TResponse>(Uri uri, string securityToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Add("authorization", securityToken);

        return SendRequest<TResponse>(request);
    }


    private Task<Result<TResponse>> Post<TResponse>(Uri uri, string securityToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri);

        request.Headers.Add("Authorization", securityToken);

        return SendRequest<TResponse>(request);
    }


    private Task<Result<TResponse>> Post<TResponse>(Uri uri, HttpContent content, string securityToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = content
        };
        request.Headers.Add("authorization", securityToken);

        return SendRequest<TResponse>(request);
    }


    private async Task<Result<TResponse>> SendRequest<TResponse>(HttpRequestMessage request)
    {
        var client = _httpClientFactory.CreateClient(HttpClientNames.IxarisClient);
        using var response = await client.SendAsync(request);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var result = JsonSerializer.Deserialize<BaseIxarisResponse<TResponse>>(responseContent);

            return result.Response is not null
                ? result.Response.Body
                : default;
        }

        try
        {
            var result = JsonSerializer.Deserialize<BaseIxarisResponse<TResponse>>(responseContent);

            var errorDetails = result?.Response?.Details?.Select(d => $"{d.Key}: {d.Value}").ToList()
                ?? new()
                {
                    result.Envelope.StatusCode
                };

            return Result.Failure<TResponse>(string.Join("; ", errorDetails));
        }
        catch (JsonException ex)
        {
            _logger.LogResponseDeserializationFailed(ex, responseContent);
            return Result.Failure<TResponse>("Response deserialization failed");
        }
    }
    

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IxarisOptions _options;
    private readonly ILogger<IxarisClient> _logger;
}