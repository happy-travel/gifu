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
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using shortid;
using shortid.Configuration;
using TokenDetails = HappyTravel.Gifu.Api.Models.AmEx.Request.TokenDetails;

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
        
        
        public async Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, string clientId, CancellationToken cancellationToken)
        {
            _logger.LogVccIssueRequestStarted(request.ReferenceCode, request.MoneyAmount.Amount, request.MoneyAmount.Currency.ToString());
            
            return await ValidateRequest()
                .Bind(CreateCard)
                .Finally(SaveResult);


            async Task<Result<AmexCurrencies>> ValidateRequest()
            {
                var validator = new InlineValidator<VccIssueRequest>();
                var today = DateTime.UtcNow.Date;

                validator.RuleFor(r => r.ActivationDate.Date).GreaterThanOrEqualTo(today);
                validator.RuleFor(r => r.DueDate.Date).GreaterThan(today);
                validator.RuleFor(r => r.MoneyAmount.Amount).GreaterThan(0);
                validator.RuleFor(r => r.ReferenceCode)
                    .NotEmpty()
                    .MustAsync(async (referenceCode, token) =>
                    {
                        return !await _context.VccIssues
                            .AnyAsync(vcc => vcc.ReferenceCode == referenceCode, token);
                    })
                    .WithMessage($"VCC for '{request.ReferenceCode}' already issued");

                var result = await validator.ValidateAsync(request, cancellationToken);

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
                            UserDefinedFieldsGroup = MapToCustomFieldList(request.ReferenceCode, request.SpecialValues)
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

        
        public async Task<Result> Delete(string referenceCode)
        {
            _logger.LogVccDeleteRequestStarted(referenceCode);

            return await GetVcc(referenceCode)
                .Bind(DeleteCard);


            async Task<Result> DeleteCard(VccIssue vcc)
            {
                var payload = new DeleteRequest
                {
                    TokenReferenceId = vcc.UniqueId
                };

                var (_, response) = await _client.Delete(payload);

                if (response.Status.ShortMessage == "success")
                {
                    _logger.LogVccDeleteRequestSuccess(referenceCode);
                    return Result.Success();
                }
                
                _logger.LogVccDeleteRequestFailure(referenceCode, response.Status.DetailedMessage);
                return Result.Failure($"Deleting VCC for `{referenceCode}` failed");
            }
        }

        public Task<Result> ModifyAmount(string referenceCode, MoneyAmount amount)
        {
            _logger.LogVccModifyAmountRequestStarted(referenceCode, amount.Amount);
            
            return GetVcc(referenceCode)
                .Bind(ValidateRequest)
                .Bind(ModifyCardAmount)
                .Bind(SaveHistory);
            
            
            Result<VccIssue> ValidateRequest(VccIssue vcc)
            {
                if (vcc.Currency != amount.Currency)
                    return Result.Failure<VccIssue>("Amount currency must be equal with VCC currency");
                
                if (amount.Amount >= vcc.Amount)
                    Result.Failure<VccIssue>("Amount must be less than VCC amount");
                
                return vcc;
            }


            async Task<Result<VccIssue>> ModifyCardAmount(VccIssue vcc)
            {
                var payload = new ModifyRequest
                {
                    TokenIssuanceParams = new ModifyAmountTokenIssuanceParams
                    {
                        TokenDetails = new ModifyAmountTokenDetails
                        {
                            TokenReferenceId = vcc.UniqueId,
                            TokenAmount = amount.ToAmExFormat()
                        }
                    }
                };
                
                var (_, response) = await _client.ModifyAmount(payload);

                if (response.Status.ShortMessage == "success")
                {
                    _logger.LogVccModifyAmountRequestSuccess(referenceCode, amount.Amount);
                    return vcc;
                }
                
                _logger.LogVccModifyAmountRequestFailure(referenceCode, response.Status.DetailedMessage);
                return Result.Failure<VccIssue>($"Modifying VCC for `{referenceCode}` failed");
            }


            async Task<Result> SaveHistory(VccIssue vcc)
            {
                var amountBefore = vcc.Amount;
                vcc.Amount = amount.Amount;
                
                _context.AmountChangesHistories.Add(new AmountChangesHistory
                {
                    VccId = vcc.UniqueId,
                    AmountAfter = amount.Amount,
                    AmountBefore = amountBefore,
                    Date = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                return Result.Success();
            }
        }


        private static string TrimCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return cardNumber;

            var cardNumberLength = cardNumber.Length;
            return cardNumber[^4..].PadLeft(cardNumberLength - 4, '*');
        }


        private async Task<Result<VccIssue>> GetVcc(string referenceCode)
        {
            var issue = await _context.VccIssues
                .SingleOrDefaultAsync(i => i.ReferenceCode == referenceCode);

            return issue ?? Result.Failure<VccIssue>($"VCC with reference code `{referenceCode}` not found");
        }

        private List<CustomField> MapToCustomFieldList(string referenceCode, Dictionary<string, string> dictionary)
        {
            var list = new List<CustomField>
            {
                new()
                {
                    Index = _fieldsIndexesMonitor.CurrentValue.BookingReferenceCodeIndex,
                    Value = referenceCode[..Math.Min(20, referenceCode.Length)]
                }
            };

            if (dictionary.TryGetValue("SupplierName", out var supplierName))
            {
                list.Add(new CustomField
                {
                    Index = _fieldsIndexesMonitor.CurrentValue.SupplierNameIndex,
                    Value = supplierName[..Math.Min(40, supplierName.Length)]
                });
            }
            
            if (dictionary.TryGetValue("AccommodationName", out var accommodationName))
            {
                list.Add(new CustomField
                {
                    Index = _fieldsIndexesMonitor.CurrentValue.AccommodationNameIndex,
                    Value = accommodationName[..Math.Min(40, accommodationName.Length)]
                });
            }
            
            return list;
        }


        private readonly IAmExClient _client;
        private readonly ILogger<VccService> _logger;
        private readonly GifuContext _context;
        private readonly AmExOptions _options;
        private readonly IOptionsMonitor<UserDefinedFieldsIndexes> _fieldsIndexesMonitor;
    }
}