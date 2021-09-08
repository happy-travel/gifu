using System;
using System.Threading.Tasks;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Api.Models.AmEx.Response;

namespace HappyTravel.Gifu.Api.Services
{
    /// <summary>
    /// Fake class to use in integration and end-to-end tests. AmEx returns the same date every time which is not suitable in some cases
    /// </summary>
    public class FakeAmexClient : IAmExClient
    {
        public Task<(string TransactionId, AmexResponse Response)> CreateToken(CreateTokenRequest payload)
        {
            var transactionId = Guid.NewGuid().ToString();
            var response = new AmexResponse()
            {
                Status = new Status { ShortMessage = "success" },
                TokenIssuanceData = new TokenIssuanceData
                {
                    TokenDetails = new Models.AmEx.Response.TokenDetails
                    {
                        TokenNumber = Guid.NewGuid().ToString(),
                        TokenSecurityCode = "777",
                        TokenExpiryDate = payload.TokenIssuanceParams.TokenDetails.TokenEndDate
                    }
                }
            };
            return Task.FromResult((transactionId, response));
        }

        public Task<(string TransactionId, AmexResponse Response)> Delete(DeleteRequest payload) 
            => Task.FromResult((string.Empty, new AmexResponse()));
    }
}