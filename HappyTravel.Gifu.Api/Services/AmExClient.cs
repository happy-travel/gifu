using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amex.Api.Client.Core.Security.Authentication;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Api.Models.AmEx.Response;
using Microsoft.Extensions.Options;

namespace HappyTravel.Gifu.Api.Services
{
    public class AmExClient : IAmExClient
    {
        public AmExClient(HttpClient httpClient, IOptions<AmExOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }
        
        
        public async Task<(string TransactionId, CreateTokenResponse Response)> CreateToken(CreateTokenRequest payload)
        {
            var endpoint = $"{_options.Endpoint}/payments/digital/v2/tokenization/smart_tokens";
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            
            await SignMessage(request);

            var response = await _httpClient.SendAsync(request);
            var result = await JsonSerializer.DeserializeAsync<CreateTokenResponse>(await response.Content.ReadAsStreamAsync());
            var transactionId = string.Empty;
            
            if (response.Headers.TryGetValues("transaction_id", out var values))
            {
                transactionId = values.Single();
            }
            
            return (transactionId, result);
        }


        private async Task SignMessage(HttpRequestMessage request)
        {
            var authProvider = new HmacAuthProvider();
            var headers = authProvider.GenerateAuthHeaders(clientKey: _options.ClientId, 
                clientSecret: _options.ClientSecret, 
                payload: request.Content is not null
                    ? await request.Content.ReadAsStringAsync()
                    : null, 
                requestUrl: request.RequestUri?.ToString());

            foreach (var (key, value) in headers)
            {
                request.Headers.Add(key, value);
            }
        }


        private readonly HttpClient _httpClient;
        private readonly AmExOptions _options;
    }
}