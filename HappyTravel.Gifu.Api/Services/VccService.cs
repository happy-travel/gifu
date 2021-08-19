using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Gifu.Api.Infrastructure.Extensions;
using HappyTravel.Gifu.Api.Infrastructure.Logging;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Models.AmEx;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Data;
using HappyTravel.Gifu.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using shortid;
using shortid.Configuration;

namespace HappyTravel.Gifu.Api.Services
{
    public class VccService : IVccService
    {
        public VccService(IAmExClient client, ILogger<VccService> logger, GifuContext context, IOptions<AmExOptions> options,
            IOptionsMonitor<UserDefinedFieldsIndexes> fieldsIndexesMonitor)
        {
            _client = client;
            _logger = logger;
            _context = context;
            _options = options.Value;
            _fieldsIndexesMonitor = fieldsIndexesMonitor;
        }
        
        
        public Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, string clientId, CancellationToken cancellationToken)
        {
            _logger.LogVccIssueRequestStarted(request.ReferenceCode, request.MoneyAmount.Amount, request.MoneyAmount.Currency.ToString());
            
            return ValidateRequest(request)
                .Bind(CreateCard)
                .Finally(SaveResult);


            static Result<AmexCurrencies> ValidateRequest(VccIssueRequest request)
            {
                var validator = new InlineValidator<VccIssueRequest>();
                var today = DateTime.UtcNow.Date;

                validator.RuleFor(r => r.ActivationDate.Date).GreaterThanOrEqualTo(today);
                validator.RuleFor(r => r.DueDate.Date).GreaterThan(today);
                validator.RuleFor(r => r.MoneyAmount.Amount).GreaterThan(0);
                validator.RuleFor(r => r.ReferenceCode).NotEmpty();

                var result = validator.Validate(request);

                if (!result.IsValid)
                    return Result.Failure<AmexCurrencies>(string.Join(";", result.Errors.Select(e => e.ErrorMessage)));

                return Enum.TryParse<AmexCurrencies>(request.MoneyAmount.Currency.ToString(), out var currency)
                    ? currency
                    : Result.Failure<AmexCurrencies>("Currency is not supported");
            }


            async Task<Result<(string, string, VirtualCreditCard)>> CreateCard(AmexCurrencies currency)
            {
                if(!_options.Accounts.TryGetValue(currency, out var accountId))
                    return Result.Failure<(string, string, VirtualCreditCard)>($"Cannot get accountId for currency `{currency}`");

                var fieldsIndexes = _fieldsIndexesMonitor.CurrentValue;
                
                var uniqueId = ShortId.Generate(new GenerationOptions
                {
                    UseNumbers = true,
                    UseSpecialCharacters = false,
                    Length = 15
                });
                
                var payload = new CreateTokenRequest
                {
                    TokenIssuanceParams = new TokenIssuanceParams
                    {
                        BillingAccountId = accountId,
                        TokenDetails = new TokenDetails
                        {
                            TokenReferenceId = uniqueId,
                            TokenAmount = request.MoneyAmount.ToAmExFormat(),
                            TokenStartDate = request.ActivationDate.ToAmExFormat(),
                            TokenEndDate = request.DueDate.ToAmExFormat()
                        },
                        ReconciliationFields = new ReconciliationFields
                        {
                            UserDefinedFieldsGroup = new List<CustomField>
                            {
                                new ()
                                {
                                    // User Id field always comes first
                                    Index = "1",
                                    Value = uniqueId
                                },
                                new ()
                                {
                                    Index = fieldsIndexes.BookingReferenceCodeIndex,
                                    Value = request.ReferenceCode
                                }
                            }
                        }
                    }
                };
                
                var (transactionId, response) = await _client.CreateToken(payload);
                
                return response.Status.ShortMessage != "success" 
                    ? Result.Failure<(string, string, VirtualCreditCard)>(response.Status.DetailedMessage) 
                    : (transactionId, uniqueId, response.TokenIssuanceData.TokenDetails.ToVirtualCreditCard());
            }

            
            async Task<Result<VirtualCreditCard>> SaveResult(Result<(string TransactionId, string UniqueId, VirtualCreditCard Vcc)> result)
            {
                if (result.IsFailure)
                {
                    _logger.LogVccIssueRequestFailure(request.ReferenceCode, result.Error);
                    return Result.Failure<VirtualCreditCard>($"Error creating VCC for reference code `{request.ReferenceCode}`");
                }
                
                _context.VccIssues.Add(new VccIssue
                {
                    TransactionId = result.Value.TransactionId,
                    UniqueId = result.Value.UniqueId,
                    ReferenceCode = request.ReferenceCode,
                    Amount = request.MoneyAmount.Amount,
                    Currency = request.MoneyAmount.Currency,
                    ActivationDate = request.ActivationDate,
                    DueDate = request.DueDate,
                    ClientId = clientId,
                    CardNumber = TrimCardNumber(result.Value.Vcc.Number)
                });
                
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogVccIssueRequestSuccess(request.ReferenceCode, result.Value.UniqueId);
                
                return result.Value.Vcc;
            }
        }


        public Task<List<VccIssue>> GetCardsInfo(List<string> referenceCodes, CancellationToken cancellationToken) 
            => _context.VccIssues
                .Where(c => referenceCodes.Contains(c.ReferenceCode))
                .ToListAsync(cancellationToken);


        private static string TrimCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return cardNumber;

            var cardNumberLength = cardNumber.Length;
            return cardNumber[^4..].PadLeft(cardNumberLength - 4, '*');
        }


        private readonly IAmExClient _client;
        private readonly ILogger<VccService> _logger;
        private readonly GifuContext _context;
        private readonly AmExOptions _options;
        private readonly IOptionsMonitor<UserDefinedFieldsIndexes> _fieldsIndexesMonitor;
    }
}