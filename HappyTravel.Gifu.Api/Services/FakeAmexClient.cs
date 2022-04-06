using System;
using System.Globalization;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Api.Models.AmEx.Response;
using HappyTravel.Gifu.Api.Services.SupplierClients;
using Microsoft.Extensions.Options;

namespace HappyTravel.Gifu.Api.Services;

/// <summary>
/// Fake class to use in integration and end-to-end tests. AmEx returns the same date every time which is not suitable in some cases
/// </summary>
public class FakeAmexClient : IAmExClient
{
    public FakeAmexClient(IOptionsMonitor<FakeAmexCardOptions> options)
    {
        _options = options;
    }

    
    public async Task<Result<(string TransactionId, TokenIssuanceData Response)>> CreateToken(CreateTokenRequest payload)
    {
        ArgumentNullException.ThrowIfNull(_options.CurrentValue.Number, nameof(_options.CurrentValue.Number));
        ArgumentNullException.ThrowIfNull(_options.CurrentValue.Cvv, nameof(_options.CurrentValue.Cvv));

        var transactionId = Guid.NewGuid().ToString();
        var response = new TokenIssuanceData
        {
            TokenDetails = new Models.AmEx.Response.TokenDetails
            {
                TokenNumber = _options.CurrentValue.Number,
                TokenSecurityCode = _options.CurrentValue.Cvv,
                TokenExpiryDate = DateTimeOffset.ParseExact(payload.TokenIssuanceParams.TokenDetails.TokenEndDate!, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyyMM")
            }
        };
        return await Task.FromResult((transactionId, response));
    }

        
    public async Task<Result<(string TransactionId, TokenIssuanceData Response)>> Remove(DeleteRequest payload) 
        => await Task.FromResult((string.Empty, new TokenIssuanceData()));

        
    public async Task<Result<(string TransactionId, TokenIssuanceData Response)>> Update(ModifyRequest payload) 
        => await Task.FromResult((string.Empty, new TokenIssuanceData()));


    private readonly IOptionsMonitor<FakeAmexCardOptions> _options;
}