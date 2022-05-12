using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Logging;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Models.Ixaris.Request;
using HappyTravel.Gifu.Api.Models.Ixaris.Response;
using HappyTravel.Gifu.Api.Services.SupplierClients;
using HappyTravel.Gifu.Api.Services.SupplierServices.IxarisServices;
using HappyTravel.Gifu.Data;
using HappyTravel.Gifu.Data.Models;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Gifu.Api.Validators;

namespace HappyTravel.Gifu.Api.Services.VccServices;

public class IxarisService : IVccSupplierService
{   
    public IxarisService(IIxarisClient client, ILogger<IxarisService> logger, IVccIssueRecordsManager vccRecordsManager, 
        IOptions<IxarisOptions> options, IVccFactoryService vccFactoryNameService, IScheduleLoadRecordsManager scheduleLoadRecordsManager)
    {            
        _client = client;
        _logger = logger;
        _vccRecordsManager = vccRecordsManager;
        _options = options.Value;
        _vccFactoryNameService = vccFactoryNameService;
        _scheduleLoadRecordsManager = scheduleLoadRecordsManager;
    }


    public async Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, MoneyAmount issuedMoneyAmount, string clientId, CancellationToken cancellationToken)
    {
        return await ValidateRequest()
            .Bind(() => Login())
            .Bind(() => _vccFactoryNameService.GetVccFactory(request.Types))
            .Bind(CreateCard)
            .Bind(GetCardDetails)
            .Bind(ScheduleLoadCard)
            .Bind(SaveResult);


        async Task<Result> ValidateRequest()
        {
            var validator = new VccIssueRequestValidator(_vccRecordsManager);
            var result = await validator.ValidateAsync(request, cancellationToken);

            return result.IsValid
                ? Result.Success()
                : Result.Failure(result.ToString(";"));
        }


        async Task<Result<(IssueVcc, CreditCardTypes)>> CreateCard(KeyValuePair<CreditCardTypes, string> vccFactory)
        {
            var issueVccRequest = new IssueVccRequest()
            {
                Currency = issuedMoneyAmount.Currency,
                FundingAccountReference = _options.Account,
                Amount = issuedMoneyAmount.Amount
            };

            var (isSuccess, _, data, error) = await _client.IssueVirtualCard(_securityToken, vccFactory.Value, issueVccRequest);

            if (isSuccess)
            {
                _logger.LogVccIssueRequestSuccess(request.ReferenceCode, data.CardReference);
                return (data, vccFactory.Key);
            }

            _logger.LogVccIssueRequestFailure(request.ReferenceCode, error);
            return Result.Failure<(IssueVcc, CreditCardTypes)>($"Error creating VCC for reference code `{request.ReferenceCode}`");
        }


        async Task<Result<(string, VccDetails, VirtualCreditCard)>> GetCardDetails((IssueVcc IssueVcc, CreditCardTypes VccType) result)
        {   
            var (isSuccess, _, response, error) = await _client.GetVirtualCardDetails(_securityToken, result.IssueVcc.CardReference);

            return isSuccess
                ? (result.IssueVcc.TransactionReference, response,
                    new(number: response.CardNumber,
                        expiry: new(int.Parse(response.ExpiryDateYear), int.Parse(response.ExpiryDateMonth), 1, 0, 0, 0, TimeSpan.Zero),
                        holder: response.CardholderName,
                        code: response.Cvv,
                        type: result.VccType))
                : Result.Failure<(string, VccDetails, VirtualCreditCard)>(error);
        }


        async Task<Result<(string, VccDetails, VirtualCreditCard)>> ScheduleLoadCard((string TransactionId, VccDetails VccDetails, VirtualCreditCard Vcc) result)
        {
            var scheduleLoadRequest = new ScheduleLoadRequest()
            {
                CardReference = result.VccDetails.CardReference,
                FundingAccountReference = _options.Account,
                Amount = issuedMoneyAmount.Amount,
                ScheduleDate = request.ActivationDate.ToString("yyyyy-MM-dd"),
                ClearanceDate = request.DueDate.ToString("yyyy-MM-dd")
            };

            var (isSuccess, _, scheduleReference, error) = await _client.ScheduleLoad(_securityToken, scheduleLoadRequest);

            if (isSuccess)
            {
                var now = DateTimeOffset.UtcNow;

                var scheduleLoad = new IxarisScheduleLoad()
                {
                    ScheduleReference = scheduleReference,
                    CardReference = result.VccDetails.CardReference,
                    Created = now,
                    Modified = now,
                    Status = IxarisScheduleLoadStatuses.Active
                };

                await _scheduleLoadRecordsManager.Add(scheduleLoad);

                return (result.TransactionId, result.VccDetails, result.Vcc);
            }

            return Result.Failure<(string, VccDetails, VirtualCreditCard)>(error);
        }


        async Task<Result<VirtualCreditCard>> SaveResult((string TransactionId, VccDetails VccDetails, VirtualCreditCard Vcc) result)
        {
            var now = DateTimeOffset.UtcNow;

            var vccIssue = new VccIssue
            {
                TransactionId = result.TransactionId,
                UniqueId = result.VccDetails.CardReference,
                ReferenceCode = request.ReferenceCode,
                Amount = request.MoneyAmount.Amount,
                Currency = request.MoneyAmount.Currency,
                IssuedAmount = issuedMoneyAmount.Amount,
                IssuedCurrency = issuedMoneyAmount.Currency,
                ActivationDate = new(int.Parse(result.VccDetails.StartDateYear), int.Parse(result.VccDetails.StartDateMonth), 1, 0, 0, 0, TimeSpan.Zero),
                DueDate = result.Vcc.Expiry,
                ClientId = clientId,
                CardNumber = result.VccDetails.CardNumber,
                Created = now,
                Modified = now,
                Status = VccStatuses.Issued,
                VccVendor = VccVendors.Ixaris
            };

            await _vccRecordsManager.Add(vccIssue);

            return result.Vcc;
        }
    }


    public async Task<Result> Remove(VccIssue Vcc)
    {
        return await Login()
            .Bind(() => _scheduleLoadRecordsManager.Get(Vcc.UniqueId))
            .Bind((ixrisScheduleLoad) => _client.CancelScheduleLoad(_securityToken, ixrisScheduleLoad.ScheduleReference))
            .Bind(RemoveCard)
            .Map(Save);


        async Task<Result<VccIssue>> RemoveCard()
        {
            var (isSuccess, _, _, error) = await _client.RemoveVirtualCard(_securityToken, Vcc.UniqueId);

            if (isSuccess)
            {
                _logger.LogVccDeleteRequestSuccess(Vcc.ReferenceCode);
                return Vcc;
            }
                
            _logger.LogVccDeleteRequestFailure(Vcc.ReferenceCode, error);
            return Result.Failure<VccIssue>($"Deleting VCC for `{Vcc.ReferenceCode}` failed");
        }


        Task Save(VccIssue vcc)
            => _vccRecordsManager.Remove(vcc);
    }


    public async Task<Result> DecreaseAmount(VccIssue Vcc, MoneyAmount amount, MoneyAmount issuedMoneyAmount)
    {
        return await Login()
            .Bind(() => _scheduleLoadRecordsManager.Get(Vcc.UniqueId))
            .Bind(DecreaseCardAmount)
            .Map(SaveHistory);


        async Task<Result<VccIssue>> DecreaseCardAmount(IxarisScheduleLoad ixarisScheduleLoad)
        {
            var updateScheduleLoadRequest = new UpdateScheduleLoadRequest()
            {
                FundingAccountReference = _options.Account,
                Amount = issuedMoneyAmount.Amount,
                ScheduleDate = Vcc.ActivationDate.ToString("yyyyy-MM-dd"),
                ClearanceDate = Vcc.DueDate.ToString("yyyyy-MM-dd")
            };

            var (isSuccess, _, _, error) = await _client.UpdateScheduleLoad(_securityToken, ixarisScheduleLoad.ScheduleReference, updateScheduleLoadRequest);

            if (isSuccess)
            {
                _logger.LogVccModifyAmountRequestSuccess(Vcc.ReferenceCode, amount.Amount);
                return Vcc;
            }

            _logger.LogVccModifyAmountRequestFailure(Vcc.ReferenceCode, error);
            return Result.Failure<VccIssue>($"Modifying VCC for `{Vcc.ReferenceCode}` failed");
        }


        Task SaveHistory(VccIssue vcc)
            => _vccRecordsManager.DecreaseAmount(vcc, amount.Amount, issuedMoneyAmount.Amount);
    }


    public Task<Result> Update(VccIssue Vcc, VccEditRequest request, MoneyAmount? issuedMoneyAmount, string clientId)
    {
        return Task.FromResult(Result.Failure("VCC editing is not available for Ixaris suppler"));
    }


    private async Task<Result> Login()
    {
        var (isSuccess, _, data, error) = await _client.Login();

        if (isSuccess)
        {
            _securityToken = data;
            return Result.Success();
        }

        return Result.Failure(error);
    }


    private string? _securityToken;
    private readonly IIxarisClient _client;
    private readonly ILogger<IxarisService> _logger;
    private readonly IVccIssueRecordsManager _vccRecordsManager;
    private readonly IxarisOptions _options;
    private readonly IVccFactoryService _vccFactoryNameService;
    private readonly IScheduleLoadRecordsManager _scheduleLoadRecordsManager;
}