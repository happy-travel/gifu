using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Logging;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models.Ixaris.Request;
using HappyTravel.Gifu.Api.Models.Ixaris.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace HappyTravel.Gifu.Api.Services.SupplierClients;

public class IxarisClient : IIxarisClient
{
    public IxarisClient(HttpClient httpClient, IOptions<IxarisOptions> options, ILogger<IxarisClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result> CancelScheduleLoad(string securityToken, string scheduleReference)
    {
        var endpoint = $"{_options.Endpoint}/ixsol-paymentpartner/schedule/{scheduleReference}/cancel";

        var (isSuccess, _, _, error) = await Post<IxarisData>(new Uri(endpoint, UriKind.Relative), securityToken);

        return isSuccess
            ? Result.Success()
            : Result.Failure(error);
    }


    public Task<Result<VccDetails>> GetVirtualCardDetails(string securityToken, string cardReference)
    {
        var endpoint = $"{_options.Endpoint}/ixsol-paymentpartner/virtualcards/card/{cardReference}";

        var requestParams = new Dictionary<string, string>() { { "getCvv", "true" } };

        return Get<VccDetails>(new Uri(endpoint, UriKind.Relative), requestParams, securityToken);
    }


    public Task<Result<IssueVcc>> IssueVirtualCard(string securityToken, string virtualCardFactoryName, IssueVccRequest issueVccRequest)
    {
        var endpoint = $"{_options.Endpoint}/ixsol-paymentpartner/virtualcards/{virtualCardFactoryName}";

        return Post<IssueVccRequest, IssueVcc>(new Uri(endpoint, UriKind.Relative), issueVccRequest, securityToken);
    }


    public async Task<Result<string>> Login()
    {
        var endpoint = $"{_options.Endpoint}/commons/auth/login";
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new FormUrlEncodedContent(new List<KeyValuePair<string?, string?>>() { // Because in the example Content-Type: application/x-www-form-urlencoded. Need to check.
                new("apiKey", _options.ApiKey),
                new("password", _options.Password)
            })
        };

        var (isSuccess, _, data, error) = await SendRequest<LoginData>(request);
        return isSuccess
            ? data.SecurityToken
            : error;
    }


    public async Task<Result<string>> RemoveVirtualCard(string securityToken, string cardReference)
    {
        var endpoint = $"{_options.Endpoint}/ixsol-paymentpartner/virtualcards/{cardReference}/delete";

        var (isSuccess, _, data, error) = await Post<IxarisData>(new Uri(endpoint, UriKind.Relative), securityToken);
        return isSuccess
            ? data.TransactionReference
            : error;
    }


    public async Task<Result<string>> ScheduleLoad(string securityToken, ScheduleLoadRequest scheduleLoadRequest)
    {
        var endpoint = $"{_options.Endpoint}/ixsol-paymentpartner/schedule/load";

        var (isSuccess, _, data, error) = await Post<ScheduleLoadRequest, IxarisData>(new Uri(endpoint, UriKind.Relative), scheduleLoadRequest, securityToken);
        return isSuccess
            ? data.ScheduleReference
            : error;
    }


    public async Task<Result<string>> UpdateScheduleLoad(string securityToken, string scheduleReference, UpdateScheduleLoadRequest updateScheduleLoadRequest)
    {
        var endpoint = $"{_options.Endpoint}/ixsol-paymentpartner/schedule/{scheduleReference}/update";

        var (isSuccess, _, data, error) = await Post<UpdateScheduleLoadRequest, IxarisData>(new Uri(endpoint, UriKind.Relative), updateScheduleLoadRequest, securityToken);
        return isSuccess
            ? data.ScheduleReference
            : error;
    }


    private Task<Result<TResponse>> Get<TResponse>(Uri uri, Dictionary<string, string> requestParams, string securityToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Add("authorization", securityToken);

        foreach(var param in requestParams)
        {
            request.Options.Set(new(param.Key), param.Value);
        }

        return SendRequest<TResponse>(request);
    }


    private Task<Result<TResponse>> Post<TResponse>(Uri uri, string securityToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri);

        request.Headers.Add("authorization", securityToken);

        return SendRequest<TResponse>(request);
    }


    private Task<Result<TResponse>> Post<TRequest, TResponse>(Uri uri, TRequest requestContext, string securityToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestContext), Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("authorization", securityToken);

        return SendRequest<TResponse>(request);
    }


    private async Task<Result<TResponse>> SendRequest<TResponse>(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);
        try
        {
            var result = await response.Content.ReadFromJsonAsync<BaseIxarisResponse<TResponse>>();

            return result.Envelope.StatusCode == "SUCCESS"
                ? result.Response.Body
                : Result.Failure<TResponse>(JsonConvert.SerializeObject(result.Response.Details.ToString())); // Details??? The type is unknown.
        }
        catch (JsonReaderException ex)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogResponseDeserializationFailed(ex, responseBody);
            return Result.Failure<TResponse>("Response deserialization failed");
        }
    }
        

    private readonly HttpClient _httpClient;
    private readonly IxarisOptions _options;
    private readonly ILogger<IxarisClient> _logger;
}