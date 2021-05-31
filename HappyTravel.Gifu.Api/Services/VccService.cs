using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Gifu.Api.Infrastructure.Extensions;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Models.AmEx;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Data;
using HappyTravel.Gifu.Data.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Gifu.Api.Services
{
    public class VccService : IVccService
    {
        public VccService(IAmExClient client, ILogger<VccService> logger, GifuContext context, IOptions<AmExOptions> options)
        {
            _client = client;
            _logger = logger;
            _context = context;
            _options = options.Value;
        }
        
        
        public Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, CancellationToken cancellationToken)
        {
            return ValidateRequest(request)
                .Bind(CreateCard)
                .Finally(WriteLog);


            static Result<AmexCurrencies> ValidateRequest(VccIssueRequest request)
            {
                var validator = new InlineValidator<VccIssueRequest>();

                validator.RuleFor(r => r.DueDate.Date).GreaterThan(DateTime.UtcNow.Date);
                validator.RuleFor(r => r.MoneyAmount.Amount).GreaterThan(0);
                validator.RuleFor(r => r.ReferenceCode).NotEmpty();

                var result = validator.Validate(request);
                
                if (!result.IsValid)
                    return Result.Failure<AmexCurrencies>(string.Join(";", result.Errors.Select(e => e.ErrorMessage)));

                return Enum.TryParse<AmexCurrencies>(request.MoneyAmount.Currency.ToString(), out var currency)
                    ? currency
                    : Result.Failure<AmexCurrencies>("Currency is not supported");
            }


            async Task<Result<(string, VirtualCreditCard)>> CreateCard(AmexCurrencies currency)
            {
                if(!_options.Accounts.TryGetValue(currency, out var accountId))
                    return Result.Failure<(string, VirtualCreditCard)>($"Cannot get accountId for currency `{currency}`");
                
                var payload = new CreateTokenRequest
                {
                    TokenIssuanceParams = new TokenIssuanceParams
                    {
                        BillingAccountId = accountId,
                        TokenDetails = new TokenDetails
                        {
                            TokenReferenceId = request.ReferenceCode,
                            TokenAmount = request.MoneyAmount.ToAmExFormat(),
                            TokenStartDate = DateTime.UtcNow.Date.ToAmExFormat(),
                            TokenEndDate = request.DueDate.ToAmExFormat()
                        }
                    }
                };
                
                var (transactionId, response) = await _client.CreateToken(payload);
                
                return response.Status.ShortMessage != "success" 
                    ? Result.Failure<(string, VirtualCreditCard)>(response.Status.DetailedMessage) 
                    : (transactionId, response.TokenIssuanceData.TokenDetails.ToVirtualCreditCard());
            }

            
            async Task<Result<VirtualCreditCard>> WriteLog(Result<(string TransactionId, VirtualCreditCard Vcc)> result)
            {
                if (result.IsFailure)
                {
                    _logger.LogError("Creating a VCC for the reference code `{ReferenceCode}` completed with the error: `{Error}`", request.ReferenceCode, result.Error);
                    return Result.Failure<VirtualCreditCard>($"Error creating VCC for reference code `{request.ReferenceCode}`");
                }
                
                _context.VccIssues.Add(new VccIssue
                {
                    TransactionId = result.Value.TransactionId,
                    ReferenceCode = request.ReferenceCode,
                    Amount = request.MoneyAmount.Amount,
                    Currency = request.MoneyAmount.Currency,
                    DueDate = request.DueDate
                });
                
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Creating a VCC for the reference code `{ReferenceCode}` completed", request.ReferenceCode);
                
                return result.Value.Vcc;
            }
        }
        

        private readonly IAmExClient _client;
        private readonly ILogger<VccService> _logger;
        private readonly GifuContext _context;
        private readonly AmExOptions _options;
    }
}