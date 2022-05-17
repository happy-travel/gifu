using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Extensions;
using HappyTravel.Gifu.Api.Infrastructure.Logging;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Infrastructure.Utils;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Api.Services.SupplierClients;
using HappyTravel.Gifu.Api.Services.VccServices;
using HappyTravel.Gifu.Api.Validators;
using HappyTravel.Gifu.Data;
using HappyTravel.Gifu.Data.Models;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Gifu.Api.Services;

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


    public async Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, MoneyAmount issuedMoneyAmount, string clientId, CancellationToken cancellationToken)
    {           
        return await ValidateRequest()
            .Bind(() => _accountsService.GetAccountId(issuedMoneyAmount.Currency))
            .Bind(CreateCard)
            .Finally(SaveResult);


        async Task<Result> ValidateRequest()
        {
            var validator = new VccIssueRequestValidator(_vccRecordsManager);
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
                amount: issuedMoneyAmount,
                startDate: request.ActivationDate,
                endDate: request.DueDate,
                customFields: _customFieldsMapper.Map(request.ReferenceCode, request.SpecialValues));
                
            var (isSuccess, _, (transactionId, response), err) = await _client.CreateToken(payload);

            return isSuccess
                ? (transactionId, uniqueId, response.TokenDetails.ToVirtualCreditCard())
                : Result.Failure<(string, string, VirtualCreditCard)>(err);
        }

            
        async Task<Result<VirtualCreditCard>> SaveResult(Result<(string TransactionId, string UniqueId, VirtualCreditCard vcc)> result)
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
                IssuedAmount = issuedMoneyAmount.Amount,
                IssuedCurrency = issuedMoneyAmount.Currency,
                ActivationDate = request.ActivationDate,
                DueDate = request.DueDate,
                ClientId = clientId,
                CardNumber = result.Value.vcc.Number,
                Created = now,
                Modified = now,
                Status = VccStatuses.Issued,
                VccVendor = VccVendors.AmericanExpress
            });
                
            _logger.LogVccIssueRequestSuccess(request.ReferenceCode, result.Value.UniqueId);
            return result.Value.vcc;
        }
    }


    public async Task<Result> Remove(VccIssue vcc)
    {
        return await GetAccountId(vcc)
            .Bind(RemoveCard)
            .Tap(Save);


        async Task<Result<VccIssue>> RemoveCard(string AccountId)
        {                
            var payload = new DeleteRequest
            {
                TokenReferenceId = vcc.UniqueId,
                BillingAccountId = AccountId
            };

            var (isSuccess, _, _, err) = await _client.Remove(payload);
            if (isSuccess)
            {
                _logger.LogVccDeleteRequestSuccess(vcc.ReferenceCode);
                return vcc;
            }
                
            _logger.LogVccDeleteRequestFailure(vcc.ReferenceCode, err);
            return Result.Failure<VccIssue>($"Deletion of a VCC for `{vcc.ReferenceCode}` has failed");
        }


        Task Save(VccIssue vcc)
            => _vccRecordsManager.Remove(vcc);
    }

        
    public async Task<Result> DecreaseAmount(VccIssue Vcc, MoneyAmount amount, MoneyAmount issuedMoneyAmount)
    {
        return await GetAccountId(Vcc)
            .Bind(DecreaseCardAmount)
            .Tap(SaveHistory);


        async Task<Result<VccIssue>> DecreaseCardAmount(string AccountId)
        {
            var payload = RequestGenerator.GenerateModifyTokenRequest(tokenNumber: Vcc.CardNumber,
                accountId: AccountId,
                tokenAmount: issuedMoneyAmount,
                tokenStartDate: null,
                tokenDueDate: null);
                
            var (isSuccess, _, _, err) = await _client.Update(payload);

            if (isSuccess)
            {
                _logger.LogVccModifyAmountRequestSuccess(Vcc.ReferenceCode, amount.Amount);
                return Vcc;
            }
                
            _logger.LogVccModifyAmountRequestFailure(Vcc.ReferenceCode, err);
            return Result.Failure<VccIssue>($"Modifying VCC for `{Vcc.ReferenceCode}` failed");
        }


        Task SaveHistory(VccIssue vcc)
            => _vccRecordsManager.DecreaseAmount(vcc, amount.Amount, issuedMoneyAmount.Amount);
    }
        
        
    public async Task<Result> Update(VccIssue vcc, VccEditRequest request, MoneyAmount? issuedMoneyAmount, string clientId)
    {
        return await IsDirectEditEnabled()               
            .Bind(() => GetAccountId(vcc))
            .Bind(UpdateCard)
            .Tap(SaveRequest);


        Result IsDirectEditEnabled()
        {
            _logger.LogVccEditRequestStarted(vcc.ReferenceCode);

            return _directEditOptionsMonitor.CurrentValue.IsEnabled
                ? Result.Success()
                : Result.Failure("VCC editing is disabled");
        }


        async Task<Result> UpdateCard(string AccountId)
        {
            var payload = RequestGenerator.GenerateModifyTokenRequest(tokenNumber: vcc.CardNumber,
                accountId: AccountId,
                tokenAmount: issuedMoneyAmount,
                tokenStartDate: request.ActivationDate,
                tokenDueDate: request.DueDate);

            var (isSuccess, _, _, err) = await _client.Update(payload);

            if (isSuccess)
            {
                _logger.LogVccEditSuccess(vcc.ReferenceCode);
            }
                
            _logger.LogVccEditFailure(vcc.ReferenceCode, err);
            return Result.Failure($"Modifying VCC for `{vcc.ReferenceCode}` failed");
        }


        Task SaveRequest()
            => _vccRecordsManager.Update(vcc, request, issuedMoneyAmount);
    }
        
        
    private Result<string> GetAccountId(VccIssue vcc)
    {
        var accountId = _accountsService.GetAccountId(vcc.IssuedCurrency);

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