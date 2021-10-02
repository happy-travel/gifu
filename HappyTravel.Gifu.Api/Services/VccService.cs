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
using HappyTravel.Money.Enums;
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
            IOptionsMonitor<UserDefinedFieldsOptions> fieldOptionsMonitor)
        {
            _client = client;
            _logger = logger;
            _context = context;
            _options = options.Value;
            _fieldOptionsMonitor = fieldOptionsMonitor;
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
                            .AnyAsync(vcc => vcc.ReferenceCode == referenceCode && vcc.Status == VccStatuses.Issued, token);
                    })
                    .WithMessage($"VCC for '{request.ReferenceCode}' already issued");

                var result = await validator.ValidateAsync(request, cancellationToken);

                return !result.IsValid 
                    ? Result.Failure<AmexCurrencies>(string.Join(";", result.Errors.Select(e => e.ErrorMessage))) 
                    : GetAmexCurrency(request.MoneyAmount.Currency);
            }


            async Task<Result<(string, string, VirtualCreditCard)>> CreateCard(AmexCurrencies currency)
            {
                if(!_options.Accounts.TryGetValue(currency, out var accountId))
                    return Result.Failure<(string, string, VirtualCreditCard)>($"Cannot get accountId for currency `{currency}`");

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
                
                var (isSuccess, _, (transactionId, response), err) = await _client.CreateToken(payload);

                return isSuccess && response.Status.ShortMessage == "success"
                    ? (transactionId, uniqueId, response.TokenIssuanceData.TokenDetails.ToVirtualCreditCard())
                    : Result.Failure<(string, string, VirtualCreditCard)>(isSuccess 
                        ? response.Status.DetailedMessage
                        : err);
            }

            
            async Task<Result<VirtualCreditCard>> SaveResult(Result<(string TransactionId, string UniqueId, VirtualCreditCard Vcc)> result)
            {
                if (result.IsFailure)
                {
                    _logger.LogVccIssueRequestFailure(request.ReferenceCode, result.Error);
                    return Result.Failure<VirtualCreditCard>($"Error creating VCC for reference code `{request.ReferenceCode}`");
                }

                var now = DateTime.UtcNow;
                
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
                    CardNumber = result.Value.Vcc.Number,
                    Created = now,
                    Modified = now,
                    Status = VccStatuses.Issued
                });
                
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogVccIssueRequestSuccess(request.ReferenceCode, result.Value.UniqueId);
                
                return result.Value.Vcc;
            }
        }


        public async Task<List<VccIssue>> GetCardsInfo(List<string> referenceCodes, CancellationToken cancellationToken)
        {
            var records = await _context.VccIssues
                .Where(c => referenceCodes.Contains(c.ReferenceCode) && c.Status == VccStatuses.Issued)
                .ToListAsync(cancellationToken);

            return records.Select(r =>
            {
                r.CardNumber = TrimCardNumber(r.CardNumber);
                return r;
            }).ToList();
        }


        public async Task<Result> Delete(string referenceCode)
        {
            _logger.LogVccDeleteRequestStarted(referenceCode);

            return await GetVcc(referenceCode)
                .Bind(DeleteCard)
                .Bind(Save);


            async Task<Result<VccIssue>> DeleteCard(VccIssue vcc)
            {
                var (_, isFailure, currency, error) = GetAmexCurrency(vcc.Currency);
                if (isFailure)
                    return Result.Failure<VccIssue>(error);
                
                if(!_options.Accounts.TryGetValue(currency, out var accountId))
                    return Result.Failure<VccIssue>($"Cannot get accountId for currency `{currency}`");
                
                var payload = new DeleteRequest
                {
                    TokenReferenceId = vcc.UniqueId,
                    BillingAccountId = accountId
                };

                var (isSuccess, _, result, err) = await _client.Delete(payload);
                if (isSuccess && result.Response.Status.ShortMessage == "success")
                {
                    _logger.LogVccDeleteRequestSuccess(referenceCode);
                    return vcc;
                }
                
                _logger.LogVccDeleteRequestFailure(referenceCode, isSuccess 
                    ? result.Response.Status.DetailedMessage
                    : err);
                
                return Result.Failure<VccIssue>($"Deleting VCC for `{referenceCode}` failed");
            }
            
            
            async Task<Result> Save(VccIssue vcc)
            {
                vcc.Modified = DateTime.UtcNow;
                vcc.Status = VccStatuses.Deleted;
                _context.Update(vcc);
                await _context.SaveChangesAsync();
                return Result.Success();
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
                    return Result.Failure<VccIssue>("Amount must be less than VCC amount");
                
                return vcc;
            }


            async Task<Result<VccIssue>> ModifyCardAmount(VccIssue vcc)
            {
                var (_, isFailure, currency, error) = GetAmexCurrency(vcc.Currency);
                if (isFailure)
                    return Result.Failure<VccIssue>(error);
                
                if(!_options.Accounts.TryGetValue(currency, out var accountId))
                    return Result.Failure<VccIssue>($"Cannot get accountId for currency `{currency}`");

                var payload = new ModifyRequest
                {
                    TokenIdentifier = new TokenIdentifier
                    {
                        TokenNumber  = vcc.CardNumber
                    },
                    TokenIssuanceParams = new ModifyAmountTokenIssuanceParams
                    {
                        BillingAccountId = accountId,
                        TokenDetails = new ModifyAmountTokenDetails
                        {
                            TokenAmount = amount.ToAmExFormat()
                        }
                    }
                };
                
                var (isSuccess, _, result, err) = await _client.ModifyAmount(payload);

                if (isSuccess && result.Response.Status.ShortMessage == "success")
                {
                    _logger.LogVccModifyAmountRequestSuccess(referenceCode, amount.Amount);
                    return vcc;
                }
                
                _logger.LogVccModifyAmountRequestFailure(referenceCode, isSuccess 
                    ? result.Response.Status.DetailedMessage
                    : err);
                
                return Result.Failure<VccIssue>($"Modifying VCC for `{referenceCode}` failed");
            }


            async Task<Result> SaveHistory(VccIssue vcc)
            {
                var amountBefore = vcc.Amount;
                vcc.Amount = amount.Amount;
                vcc.Modified = DateTime.UtcNow;
                
                _context.AmountChangesHistories.Add(new AmountChangesHistory
                {
                    VccId = vcc.UniqueId,
                    AmountAfter = amount.Amount,
                    AmountBefore = amountBefore,
                    Date = DateTime.UtcNow
                });

                _context.Update(vcc);
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
                .Where(i => i.Status == VccStatuses.Issued)
                .SingleOrDefaultAsync(i => i.ReferenceCode == referenceCode);

            return issue ?? Result.Failure<VccIssue>($"VCC with reference code `{referenceCode}` not found");
        }

        private List<CustomField> MapToCustomFieldList(string referenceCode, Dictionary<string, string> dictionary)
        {
            var fieldsOptions = _fieldOptionsMonitor.CurrentValue;
            
            var list = new List<CustomField>
            {
                new()
                {
                    Index = fieldsOptions.BookingReferenceCode.Index,
                    Value = referenceCode[..Math.Min(fieldsOptions.BookingReferenceCode.Length, referenceCode.Length)]
                }
            };

            foreach (var (key, value) in dictionary)
            {
                if (fieldsOptions.CustomFields.TryGetValue(key, out var fieldSettings))
                {
                    list.Add(new CustomField
                    {
                        Index = fieldSettings.Index,
                        Value = value[..Math.Min(fieldSettings.Length, value.Length)]
                    });
                }
            }
            
            return list;
        }


        private static Result<AmexCurrencies> GetAmexCurrency(Currencies currency) 
            => currency switch
            {
                Currencies.USD => AmexCurrencies.USD,
                Currencies.AED => AmexCurrencies.AED,
                _ => Result.Failure<AmexCurrencies>("Currency is not supported")
            };


        private readonly IAmExClient _client;
        private readonly ILogger<VccService> _logger;
        private readonly GifuContext _context;
        private readonly AmExOptions _options;
        private readonly IOptionsMonitor<UserDefinedFieldsOptions> _fieldOptionsMonitor;
    }
}