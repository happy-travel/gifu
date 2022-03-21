using System;
using System.Globalization;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Api.Models.AmEx.Response;
using HappyTravel.Gifu.Api.Services.SupplierClients;

namespace HappyTravel.Gifu.Api.Services;

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
                TokenNumber = "378282246310005", // Fake, but valid AmEx card number
                TokenSecurityCode = "777",
                TokenExpiryDate = DateTimeOffset.ParseExact(payload.TokenIssuanceParams.TokenDetails.TokenEndDate!, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyyMM")
            }
        };
        return await Task.FromResult((transactionId, response));
    }

        
    public async Task<Result<(string TransactionId, TokenIssuanceData Response)>> Remove(DeleteRequest payload) 
        => await Task.FromResult((string.Empty, new TokenIssuanceData()));

        
    public async Task<Result<(string TransactionId, TokenIssuanceData Response)>> Update(ModifyRequest payload) 
        => await Task.FromResult((string.Empty, new TokenIssuanceData()));
}