using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amex.Api.Client.Core.Security.Authentication;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Extensions;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Api.Models.AmEx.Response;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Options;
using TokenDetails = HappyTravel.Gifu.Api.Models.AmEx.Request.TokenDetails;

namespace HappyTravel.Gifu.Api.Services
{
    public class AmExClient : IAmExClient
    {
        public AmExClient(HttpClient httpClient, IOptions<AmExOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }
        
        
        public async Task<Result<VccInfo>> CreateCard(string referenceCode, MoneyAmount moneyAmount, DateTime dueDate)
        {
            var endpoint = $"{_options.Endpoint}/payments/digital/v2/tokenization/smart_tokens";
            var payload = new CreateTokenRequest
            {
                TokenIssuanceParams = new TokenIssuanceParams
                {
                    TokenDetails = new TokenDetails
                    {
                        TokenReferenceId = referenceCode,
                        TokenAmount = moneyAmount.ToAmExFormat(),
                        TokenStartDate = DateTime.UtcNow.Date.ToAmExFormat(),
                        TokenEndDate = dueDate.ToAmExFormat()
                    }
                }
            };
            
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            
            request.Headers.Add("transaction_id", referenceCode);
            await SignMessage(request);

            var response = await _httpClient.SendAsync(request);
            var content = await JsonSerializer.DeserializeAsync<CreateTokenResponse>(await response.Content.ReadAsStreamAsync());

            return content.Status.ShortMessage != "success"
                ? Result.Failure<VccInfo>(content.Status.DetailedMessage)
                : content.TokenIssuanceData.TokenDetails.ToVccInfo();
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