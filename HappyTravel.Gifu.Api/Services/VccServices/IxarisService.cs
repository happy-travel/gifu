using CSharpFunctionalExtensions;
using FluentValidation;
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
using System.Threading;
using System.Threading.Tasks;

namespace HappyTravel.Gifu.Api.Services.VccServices
{
    public class IxarisService : IVccSupplierService
    {   
        public IxarisService(IIxarisClient client, ILogger<IxarisService> logger, IVccIssueRecordsManager vccRecordsManager, 
            IOptions<IxarisOptions> options, IVccFactoryNameService vccFactoryNameService, IScheduleLoadRecordsManager scheduleLoadRecordsManager)
        {            
            _client = client;
            _logger = logger;
            _vccRecordsManager = vccRecordsManager;
            _options = options.Value;
            _vccFactoryNameService = vccFactoryNameService;
            _scheduleLoadRecordsManager = scheduleLoadRecordsManager;
        }


        public async Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, string clientId, CancellationToken cancellationToken)
        {
            return await ValidateRequest()
                .Bind(() => Login())
                .Bind(() => _vccFactoryNameService.GetVccFactoryName(request.Type))
                .Bind(CreateCard)
                .Bind(GetCardDetails)
                .Bind(ScheduleLoadCard)
                .Bind(SaveResult);


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


            async Task<Result<IssueVcc>> CreateCard(string vccFactoryName)
            {
                var issueVccRequest = new IssueVccRequest()
                {
                    Currency = request.MoneyAmount.Currency,
                    FundingAccountReference = _options.Account,
                    Amount = request.MoneyAmount.Amount
                };

                var (isSuccess, _, data, error) = await _client.IssueVirtualCard(_securityToken, vccFactoryName, issueVccRequest);

                if (isSuccess)
                {
                    _logger.LogVccIssueRequestSuccess(request.ReferenceCode, data.CardReference);
                    return data;
                }

                _logger.LogVccIssueRequestFailure(request.ReferenceCode, error);
                return Result.Failure<IssueVcc>($"Error creating VCC for reference code `{request.ReferenceCode}`");
            }


            async Task<Result<(string, VccDetails)>> GetCardDetails(IssueVcc issueVccResponse)
            {   
                var (isSuccess, _, data, error) = await _client.GetVirtualCardDetails(_securityToken, issueVccResponse.CardReference);

                return isSuccess
                    ? (issueVccResponse.TransactionReference, data)
                    : Result.Failure<(string, VccDetails)>(error);
            }


            async Task<Result<(string, VccDetails)>> ScheduleLoadCard((string TransactionReference, VccDetails VccDetails) result)
            {
                var scheduleLoadRequest = new ScheduleLoadRequest()
                {
                    CardReference = result.VccDetails.CardReference,
                    FundingAccountReference = _options.Account,
                    Amount = request.MoneyAmount.Amount,
                    ScheduleDate = request.ActivationDate.ToString("yyyyy-MM-dd"),
                    ClearanceDate = request.DueDate.ToString("yyyy-MM-dd")
                };

                var (isSuccess, _, scheduleReference, error) = await _client.ScheduleLoad(_securityToken, scheduleLoadRequest);

                if (isSuccess)
                {
                    var now = DateTime.UtcNow;

                    var scheduleLoad = new IxarisScheduleLoad()
                    {
                        ScheduleReference = scheduleReference,
                        CardReference = result.VccDetails.CardReference,
                        Created = now,
                        Modified = now,
                        Status = IxarisScheduleLoadStatuses.Active
                    };

                    await _scheduleLoadRecordsManager.Add(scheduleLoad);

                    return (result.TransactionReference, result.VccDetails);
                }

                return Result.Failure<(string, VccDetails)>(error);
            }


            async Task<Result<VirtualCreditCard>> SaveResult((string TransactionId, VccDetails VccCardDetails) result)
            {
                var now = DateTime.UtcNow;

                var vccIssue = new VccIssue
                {
                    TransactionId = result.TransactionId,
                    UniqueId = result.VccCardDetails.CardReference,
                    ReferenceCode = request.ReferenceCode,
                    Amount = request.MoneyAmount.Amount,
                    Currency = request.MoneyAmount.Currency,
                    ActivationDate = new(int.Parse(result.VccCardDetails.StartDateYear), int.Parse(result.VccCardDetails.StartDateMonth), 1),
                    DueDate = new(int.Parse(result.VccCardDetails.ExpiryDateYear), int.Parse(result.VccCardDetails.ExpiryDateMonth), 1),
                    ClientId = clientId,
                    CardNumber = result.VccCardDetails.CardNumber,
                    Created = now,
                    Modified = now,
                    Status = VccStatuses.Issued,
                    VccVendor = VccVendors.Ixaris
                };

                await _vccRecordsManager.Add(vccIssue);
                                
                return new VirtualCreditCard(number: vccIssue.CardNumber,
                        expiry: vccIssue.DueDate,
                        holder: result.VccCardDetails.CardholderName,
                        code: result.VccCardDetails.Cvv,
                        type: request.Type ?? _options.DefaultVccType);
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


        public async Task<Result> DecreaseAmount(VccIssue Vcc, MoneyAmount amount)
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
                    Amount = amount.Amount,
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
                => _vccRecordsManager.DecreaseAmount(vcc, amount.Amount);
        }


        public Task<Result> Update(VccIssue Vcc, VccEditRequest request, string clientId)
        {
            return Task.Run(() => Result.Failure("VCC editing is not available for Ixaris suppler"));
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
        private readonly IVccFactoryNameService _vccFactoryNameService;
        private readonly IScheduleLoadRecordsManager _scheduleLoadRecordsManager;
    }
}
