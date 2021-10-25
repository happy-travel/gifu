using System;
using System.Globalization;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Api.Models.AmEx.Response;

namespace HappyTravel.Gifu.Api.Services
{
    /// <summary>
    /// Fake class to use in integration and end-to-end tests. AmEx returns the same date every time which is not suitable in some cases
    /// </summary>
    public class FakeAmexClient : IAmExClient
    {
        public async Task<Result<(string TransactionId, TokenIssuanceData Response)>> CreateToken(CreateTokenRequest payload)
        {
            var transactionId = Guid.NewGuid().ToString();
            var response = new TokenIssuanceData
            {
                TokenDetails = new Models.AmEx.Response.TokenDetails
                {
                    TokenNumber = Guid.NewGuid().ToString(),
                    TokenSecurityCode = "777",
                    TokenExpiryDate = DateTime.ParseExact(payload.TokenIssuanceParams.TokenDetails.TokenEndDate!, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyyMM")
                }
            };
            return await Task.FromResult((transactionId, response));
        }

        
        public async Task<Result<(string TransactionId, TokenIssuanceData Response)>> Delete(DeleteRequest payload) 
            => await Task.FromResult((string.Empty, new TokenIssuanceData()));

        
        public async Task<Result<(string TransactionId, TokenIssuanceData Response)>> Edit(ModifyRequest payload) 
            => await Task.FromResult((string.Empty, new TokenIssuanceData()));
    }
}