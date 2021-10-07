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
using HappyTravel.Gifu.Api.Infrastructure.Utils;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Data;
using HappyTravel.Gifu.Data.Models;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Gifu.Api.Services
{
    public class VccService : IVccService
    {
        public VccService(IAmExClient client, ILogger<VccService> logger, IVccIssueRecordsManager vccRecordsManager, IOptions<AmExOptions> options,
            ICustomFieldsMapper customFieldsMapper, IOptionsMonitor<DirectEditOptions> directEditOptionsMonitor)
        {
            _client = client;
            _logger = logger;
            _vccRecordsManager = vccRecordsManager;
            _options = options.Value;
            _customFieldsMapper = customFieldsMapper;
            _directEditOptionsMonitor = directEditOptionsMonitor;
        }
        
        
        public async Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, string clientId, CancellationToken cancellationToken)
        {
            _logger.LogVccIssueRequestStarted(request.ReferenceCode, request.MoneyAmount.Amount, request.MoneyAmount.Currency.ToString());
            
            return await ValidateRequest()
                .Bind(CreateCard)
                .Finally(SaveResult);


            async Task<Result> ValidateRequest()
            {
                var validator = new InlineValidator<VccIssueRequest>();
                var today = DateTime.UtcNow.Date;

                validator.RuleFor(r => r.ActivationDate.Date).GreaterThanOrEqualTo(today);
                validator.RuleFor(r => r.DueDate.Date).GreaterThan(today);
                validator.RuleFor(r => r.MoneyAmount.Amount).GreaterThan(0);
                validator.RuleFor(r => r.ReferenceCode)
                    .NotEmpty()
                    .MustAsync(async (referenceCode, _) => !await _vccRecordsManager.IsIssued(referenceCode))
                    .WithMessage($"VCC for '{request.ReferenceCode}' already issued");

                var result = await validator.ValidateAsync(request, cancellationToken);

                return result.IsValid
                    ? Result.Success()
                    : Result.Failure(result.ToString(";"));
            }


            async Task<Result<(string, string, VirtualCreditCard)>> CreateCard()
            {
                var uniqueId = UniqueIdGenerator.Get();
                var payload = RequestGenerator.CreateTokenRequest(uniqueId: uniqueId,
                    accountId: _options.Accounts[request.MoneyAmount.Currency],
                    amount: request.MoneyAmount,
                    startDate: request.ActivationDate,
                    endDate: request.DueDate,
                    customFields: _customFieldsMapper.Map(request.ReferenceCode, request.SpecialValues));
                
                var (isSuccess, _, (transactionId, response), err) = await _client.CreateToken(payload);

                return isSuccess
                    ? (transactionId, uniqueId, response.TokenDetails.ToVirtualCreditCard())
                    : Result.Failure<(string, string, VirtualCreditCard)>(err);
            }

            
            async Task<Result<VirtualCreditCard>> SaveResult(Result<(string TransactionId, string UniqueId, VirtualCreditCard Vcc)> result)
            {
                if (result.IsFailure)
                {
                    _logger.LogVccIssueRequestFailure(request.ReferenceCode, result.Error);
                    return Result.Failure<VirtualCreditCard>($"Error creating VCC for reference code `{request.ReferenceCode}`");
                }

                var now = DateTime.UtcNow;
                
                await _vccRecordsManager.Add(new VccIssue
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
                
                _logger.LogVccIssueRequestSuccess(request.ReferenceCode, result.Value.UniqueId);
                return result.Value.Vcc;
            }
        }


        public async Task<List<VccIssue>> GetCardsInfo(List<string> referenceCodes, CancellationToken cancellationToken) 
            => (await _vccRecordsManager.Get(referenceCodes)).TrimCardNumbers().ToList();


        public async Task<Result> Delete(string referenceCode)
        {
            _logger.LogVccDeleteRequestStarted(referenceCode);

            return await _vccRecordsManager.Get(referenceCode)
                .Bind(DeleteCard)
                .Map(Save);


            async Task<Result<VccIssue>> DeleteCard(VccIssue vcc)
            {
                var payload = new DeleteRequest
                {
                    TokenReferenceId = vcc.UniqueId,
                    BillingAccountId = _options.Accounts[vcc.Currency]
                };

                var (isSuccess, _, _, err) = await _client.Delete(payload);
                if (isSuccess)
                {
                    _logger.LogVccDeleteRequestSuccess(referenceCode);
                    return vcc;
                }
                
                _logger.LogVccDeleteRequestFailure(referenceCode, err);
                return Result.Failure<VccIssue>($"Deleting VCC for `{referenceCode}` failed");
            }


            Task Save(VccIssue vcc)
                => _vccRecordsManager.Delete(vcc);
        }

        
        public async Task<Result> ModifyAmount(string referenceCode, MoneyAmount amount)
        {
            _logger.LogVccModifyAmountRequestStarted(referenceCode, amount.Amount);
            
            return await _vccRecordsManager.Get(referenceCode)
                .Bind(ValidateRequest)
                .Bind(ModifyCardAmount)
                .Map(SaveHistory);
            
            
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
                var payload = RequestGenerator.CreateModifyRequest(tokenNumber: vcc.CardNumber,
                    accountId: _options.Accounts[vcc.Currency],
                    tokenAmount: amount,
                    tokenStartDate: null,
                    tokenDueDate: null);
                
                var (isSuccess, _, _, err) = await _client.Edit(payload);

                if (isSuccess)
                {
                    _logger.LogVccModifyAmountRequestSuccess(referenceCode, amount.Amount);
                    return vcc;
                }
                
                _logger.LogVccModifyAmountRequestFailure(referenceCode, err);
                return Result.Failure<VccIssue>($"Modifying VCC for `{referenceCode}` failed");
            }


            Task SaveHistory(VccIssue vcc)
                => _vccRecordsManager.ModifyAmount(vcc, amount.Amount);
        }
        
        
        public async Task<Result> Edit(string referenceCode, VccEditRequest request, string clientId)
        {
            return await IsDirectEditEnabled()
                .Bind(() => _vccRecordsManager.Get(referenceCode))
                .Bind(Validate)
                .Bind(EditCard)
                .Map(SaveRequest);


            Result IsDirectEditEnabled()
            {
                _logger.LogVccEditRequestStarted(referenceCode);

                return _directEditOptionsMonitor.CurrentValue.IsEnabled
                    ? Result.Success()
                    : Result.Failure("VCC editing is disabled");
            }


            Result<VccIssue> Validate(VccIssue vcc)
            {
                if (request.ActivationDate is null && request.DueDate is null && request.MoneyAmount is null)
                    return Result.Failure<VccIssue>("At least one field must be filled");

                if (request.MoneyAmount is not null && request.MoneyAmount.Value.Currency != vcc.Currency)
                    return Result.Failure<VccIssue>("Currency does not match with VCC currency");

                return vcc;
            }


            async Task<Result<VccIssue>> EditCard(VccIssue vcc)
            {
                var payload = RequestGenerator.CreateModifyRequest(tokenNumber: vcc.CardNumber,
                    accountId: _options.Accounts[vcc.Currency],
                    tokenAmount: request.MoneyAmount,
                    tokenStartDate: request.ActivationDate,
                    tokenDueDate: request.DueDate);

                var (isSuccess, _, _, err) = await _client.Edit(payload);

                if (isSuccess)
                {
                    _logger.LogVccEditSuccess(referenceCode);
                    return vcc;
                }
                
                _logger.LogVccEditFailure(referenceCode, err);
                return Result.Failure<VccIssue>($"Modifying VCC for `{referenceCode}` failed");
            }


            Task SaveRequest(VccIssue vcc)
                => _vccRecordsManager.Edit(vcc, request);
        }


        private readonly IAmExClient _client;
        private readonly ILogger<VccService> _logger;
        private readonly AmExOptions _options;
        private readonly IVccIssueRecordsManager _vccRecordsManager;
        private readonly ICustomFieldsMapper _customFieldsMapper;
        private readonly IOptionsMonitor<DirectEditOptions> _directEditOptionsMonitor;
    }
}