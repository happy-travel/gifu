using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Amex.Api.Client.Core.Security.Authentication;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Logging;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Api.Models.AmEx.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace HappyTravel.Gifu.Api.Services
{
    public class AmExClient : IAmExClient
    {
        public AmExClient(HttpClient httpClient, IOptions<AmExOptions> options, ILogger<AmExClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }
        
        
        public Task<Result<(string TransactionId, AmexResponse Response)>> CreateToken(CreateTokenRequest payload) 
            => SendRequest(HttpMethod.Post, payload);


        public Task<Result<(string TransactionId, AmexResponse Response)>> Delete(DeleteRequest payload) 
            => SendRequest(HttpMethod.Delete, payload);


        public Task<Result<(string TransactionId, AmexResponse Response)>> Edit(ModifyRequest payload)
            => SendRequest(HttpMethod.Put, payload);


        private async Task<Result<(string TransactionId, AmexResponse Response)>> SendRequest<T>(HttpMethod httpMethod, T payload)
        {
            var endpoint = $"{_options.Endpoint}/payments/digital/v2/tokenization/smart_tokens";
            var request = new HttpRequestMessage(httpMethod, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            
            await SignMessage(httpMethod, request);
            
            var response = await _httpClient.SendAsync(request);
            try
            {
                var result = await response.Content.ReadFromJsonAsync<AmexResponse>();
                var transactionId = string.Empty;

                if (response.Headers.TryGetValues("transaction_id", out var values))
                {
                    transactionId = values.Single();
                }

                return (transactionId, result);
            }
            catch (JsonReaderException ex)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogResponseDeserializationFailed(responseBody, ex);
                return Result.Failure<(string TransactionId, AmexResponse Response)>("Response deserialization failed");
            }
        }


        private async Task SignMessage(HttpMethod httpMethod, HttpRequestMessage request)
        {
            var authProvider = new HmacAuthProvider();
            var headers = authProvider.GenerateAuthHeaders(clientKey: _options.ClientId, 
                clientSecret: _options.ClientSecret, 
                payload: request.Content is not null
                    ? await request.Content.ReadAsStringAsync()
                    : null, 
                requestUrl: request.RequestUri?.ToString(),
                httpMethod: httpMethod.Method);

            foreach (var (key, value) in headers)
            {
                request.Headers.Add(key, value);
            }
        }


        private readonly HttpClient _httpClient;
        private readonly AmExOptions _options;
        private readonly ILogger<AmExClient> _logger;
    }
}