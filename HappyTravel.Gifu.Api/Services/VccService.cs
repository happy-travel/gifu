using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Gifu.Api.Infrastructure.Extensions;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Models.AmEx;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Data;
using HappyTravel.Gifu.Data.Models;
using HappyTravel.Money.Enums;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Gifu.Api.Services
{
    public class VccService : IVccService
    {
        public VccService(IAmExClient client, ILogger<VccService> logger, GifuContext context)
        {
            _client = client;
            _logger = logger;
            _context = context;
        }
        
        
        public Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, CancellationToken cancellationToken)
        {
            return ValidateRequest(request)
                .Bind(CreateCard)
                .Finally(WriteLog);


            static Result ValidateRequest(VccIssueRequest request)
            {
                var validator = new InlineValidator<VccIssueRequest>();

                validator.RuleFor(r => r.DueDate.Date).GreaterThan(DateTime.UtcNow.Date);
                validator.RuleFor(r => r.MoneyAmount.Currency).Must(IsSupported).WithMessage("Currency is not supported");
                validator.RuleFor(r => r.MoneyAmount.Amount).GreaterThan(0);
                validator.RuleFor(r => r.ReferenceCode).NotEmpty();

                var result = validator.Validate(request);

                return result.IsValid
                    ? Result.Success()
                    : Result.Failure(string.Join(";", result.Errors.Select(e => e.ErrorMessage)));


                static bool IsSupported(Currencies currency) 
                    => Enum.GetNames(typeof(AmexCurrencies))
                        .Any(x => x.Equals(currency.ToString(), StringComparison.OrdinalIgnoreCase));
            }


            async Task<Result<(string, VirtualCreditCard)>> CreateCard()
            {
                var payload = new CreateTokenRequest
                {
                    TokenIssuanceParams = new TokenIssuanceParams
                    {
                        BillingAccountId = string.Empty, // TODO: set account id for currency in request
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
    }
}