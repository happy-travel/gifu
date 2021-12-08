using System;
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
using HappyTravel.Gifu.Api.Services.SupplierClients;
using HappyTravel.Gifu.Api.Services.VccServices;
using HappyTravel.Gifu.Data;
using HappyTravel.Gifu.Data.Models;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Gifu.Api.Services
{
    public class AmExService : IVccSupplierService
    {   
        public AmExService(IAmExClient client, ILogger<AmExService> logger, IVccIssueRecordsManager vccRecordsManager,
            ICustomFieldsMapper customFieldsMapper, IOptionsMonitor<DirectEditOptions> directEditOptionsMonitor, IAccountsService accountsService)
        {            
            _client = client;
            _logger = logger;
            _vccRecordsManager = vccRecordsManager;
            _customFieldsMapper = customFieldsMapper;
            _directEditOptionsMonitor = directEditOptionsMonitor;
            _accountsService = accountsService;
        }


        public async Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, string clientId, CancellationToken cancellationToken)
        {           
            return await ValidateRequest()
                .Bind(() => _accountsService.GetAccountId(request.MoneyAmount.Currency))
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
                    .WithMessage($"A VCC for '{request.ReferenceCode}' was already issued");

                var result = await validator.ValidateAsync(request, cancellationToken);

                return result.IsValid
                    ? Result.Success()
                    : Result.Failure(result.ToString(";"));
            }


            async Task<Result<(string, string, VirtualCreditCard)>> CreateCard(string accountId)
            {
                var uniqueId = UniqueIdGenerator.Get();
                var payload = RequestGenerator.GenerateCreateTokenRequest(uniqueId: uniqueId,
                    accountId: accountId,
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
                    Status = VccStatuses.Issued,
                    VccVendor = VccVendors.AmericanExpress
                });
                
                _logger.LogVccIssueRequestSuccess(request.ReferenceCode, result.Value.UniqueId);
                return result.Value.Vcc;
            }
        }


        public async Task<Result> Delete(VccIssue Vcc)
        {
            return await GetAccountId(Vcc)
                .Bind(DeleteCard)
                .Map(Save);


            async Task<Result<VccIssue>> DeleteCard(string AccountId)
            {                
                var payload = new DeleteRequest
                {
                    TokenReferenceId = Vcc.UniqueId,
                    BillingAccountId = AccountId
                };

                var (isSuccess, _, _, err) = await _client.Delete(payload);
                if (isSuccess)
                {
                    _logger.LogVccDeleteRequestSuccess(Vcc.ReferenceCode);
                    return Vcc;
                }
                
                _logger.LogVccDeleteRequestFailure(Vcc.ReferenceCode, err);
                return Result.Failure<VccIssue>($"Deletion of a VCC for `{Vcc.ReferenceCode}` has failed");
            }


            Task Save(VccIssue vcc)
                => _vccRecordsManager.Delete(vcc);
        }

        
        public async Task<Result> ModifyAmount(VccIssue Vcc, MoneyAmount amount)
        {
            return await GetAccountId(Vcc)
                .Bind(ModifyCardAmount)
                .Map(SaveHistory);


            async Task<Result<VccIssue>> ModifyCardAmount(string AccountId)
            {                
                var payload = RequestGenerator.GenerateModifyTokenRequest(tokenNumber: Vcc.CardNumber,
                    accountId: AccountId,
                    tokenAmount: amount,
                    tokenStartDate: null,
                    tokenDueDate: null);
                
                var (isSuccess, _, _, err) = await _client.Edit(payload);

                if (isSuccess)
                {
                    _logger.LogVccModifyAmountRequestSuccess(Vcc.ReferenceCode, amount.Amount);
                    return Vcc;
                }
                
                _logger.LogVccModifyAmountRequestFailure(Vcc.ReferenceCode, err);
                return Result.Failure<VccIssue>($"Modifying VCC for `{Vcc.ReferenceCode}` failed");
            }


            Task SaveHistory(VccIssue vcc)
                => _vccRecordsManager.ModifyAmount(vcc, amount.Amount);
        }
        
        
        public async Task<Result> Edit(VccIssue Vcc, VccEditRequest request, string clientId)
        {
            return await IsDirectEditEnabled()
                .Bind(Validate)
                .Bind(GetAccountId)
                .Bind(EditCard)
                .Map(SaveRequest);


            Result IsDirectEditEnabled()
            {
                _logger.LogVccEditRequestStarted(Vcc.ReferenceCode);

                return _directEditOptionsMonitor.CurrentValue.IsEnabled
                    ? Result.Success()
                    : Result.Failure("VCC editing is disabled");
            }


            Result<VccIssue> Validate()
            {
                if (request.ActivationDate is null && request.DueDate is null && request.MoneyAmount is null)
                    return Result.Failure<VccIssue>("At least one field must be filled");

                if (request.MoneyAmount is not null && request.MoneyAmount.Value.Currency != Vcc.Currency)
                    return Result.Failure<VccIssue>("Currency does not match with VCC currency");

                return Vcc;
            }


            async Task<Result> EditCard(string AccountId)
            {
                var payload = RequestGenerator.GenerateModifyTokenRequest(tokenNumber: Vcc.CardNumber,
                    accountId: AccountId,
                    tokenAmount: request.MoneyAmount,
                    tokenStartDate: request.ActivationDate,
                    tokenDueDate: request.DueDate);

                var (isSuccess, _, _, err) = await _client.Edit(payload);

                if (isSuccess)
                {
                    _logger.LogVccEditSuccess(Vcc.ReferenceCode);
                }
                
                _logger.LogVccEditFailure(Vcc.ReferenceCode, err);
                return Result.Failure($"Modifying VCC for `{Vcc.ReferenceCode}` failed");
            }


            Task SaveRequest()
                => _vccRecordsManager.Edit(Vcc, request);
        }
        
        
        private Result<string> GetAccountId(VccIssue vcc)
        {
            var accountId = _accountsService.GetAccountId(vcc.Currency);

            return accountId.IsSuccess
                ? accountId.Value
                : Result.Failure<string>(accountId.Error);
        }


        private readonly IAmExClient _client;
        private readonly ILogger<AmExService> _logger;
        private readonly IVccIssueRecordsManager _vccRecordsManager;
        private readonly ICustomFieldsMapper _customFieldsMapper;
        private readonly IAccountsService _accountsService;
        private readonly IOptionsMonitor<DirectEditOptions> _directEditOptionsMonitor;
    }
}