using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Logging;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Services.CurrencyConverter;
using HappyTravel.Gifu.Data.Models;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HappyTravel.Gifu.Api.Services.VccServices;

public class VccService : IVccService
{
    public VccService(ILogger<VccService> logger, IOptions<VccServiceOptions> options,
        IVccIssueRecordsManager vccRecordsManager, VccServiceResolver serviceResolver, CurrencyConverterService currencyConverterService)
    {
        _logger = logger;
        _options = options.Value;
        _vccRecordsManager = vccRecordsManager;
        _serviceResolver = serviceResolver;
        _currencyConverterService = currencyConverterService;
    }


    public async Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, string clientId, CancellationToken cancellationToken)
    {
        _logger.LogVccIssueRequestStarted(request.ReferenceCode, request.MoneyAmount.Amount, request.MoneyAmount.Currency.ToString());

        MoneyAmount issuedMoneyAmount;
        var (isSuccess, _, vccService, error) = ResolveService(request.MoneyAmount.Currency);

        if (isSuccess)
        {
            issuedMoneyAmount = request.MoneyAmount;
            return await vccService.Issue(request, issuedMoneyAmount, clientId, cancellationToken);
        }

        if (_options.CurrenciesToConvert.TryGetValue(request.MoneyAmount.Currency, out var issuedCurrency))
        {
            var (getRateSuccess, _, conversionResult, getRateError) = await _currencyConverterService.ConvertToCurrency(request.MoneyAmount, issuedCurrency);

            if (getRateSuccess)
            {
                issuedMoneyAmount = conversionResult;

                return await ResolveService(issuedCurrency)
                    .Bind(vccService => vccService.Issue(request, issuedMoneyAmount, clientId, cancellationToken));
            }
            
            return Result.Failure<VirtualCreditCard>(getRateError);
        }

        return Result.Failure<VirtualCreditCard>(error);


        Result<IVccSupplierService> ResolveService(Currencies currency)
            => _serviceResolver.ResolveService(request.Types, currency);
    }


    public async Task<List<VccIssue>> GetCardsInfo(List<string> referenceCodes, CancellationToken cancellationToken)
        => (await _vccRecordsManager.Get(referenceCodes)).MaskCardNumbers().ToList();


    public async Task<Result> Remove(string referenceCode)
    {
        _logger.LogVccDeleteRequestStarted(referenceCode);

        return await _vccRecordsManager.Get(referenceCode)
            .Bind(GetVccService)
            .Bind(RemoveCard);
            

        Task<Result> RemoveCard((VccIssue Vcc, IVccSupplierService vccService) result)
            => result.vccService.Remove(result.Vcc);
    }


    public async Task<Result> DecreaseAmount(string referenceCode, MoneyAmount amount)
    {
        _logger.LogVccModifyAmountRequestStarted(referenceCode, amount.Amount);

        if (amount.Amount == 0m)
            return Result.Success();

        return await _vccRecordsManager.Get(referenceCode)
            .Bind(ValidateRequest)
            .Bind(GetVccService)
            .Bind(DecreaseCardAmount);


        Result<VccIssue> ValidateRequest(VccIssue vcc)
        {
            if (vcc.Currency != amount.Currency)
                return Result.Failure<VccIssue>("Amount currency must be equal with VCC currency");

            if (amount.Amount >= vcc.Amount)
                return Result.Failure<VccIssue>("Amount must be less than VCC amount");

            return vcc;
        }


        async Task<Result> DecreaseCardAmount((VccIssue, IVccSupplierService) result)
        {
            var (vcc, vccService) = result;

            if (vcc.IssuedCurrency == amount.Currency)
                return await vccService.DecreaseAmount(vcc, amount, amount);

            return await _currencyConverterService.ConvertToCurrency(amount, vcc.IssuedCurrency)
                .Bind((issuedMoneyAmount) => vccService.DecreaseAmount(vcc, amount, issuedMoneyAmount));
        }
    }


    public async Task<Result> Update(string referenceCode, VccEditRequest request, string clientId)
    {
        return await _vccRecordsManager.Get(referenceCode)
            .Bind(ValidateRequest)
            .Bind(GetVccService)
            .Bind(UpdateCard);


        Result<VccIssue> ValidateRequest(VccIssue vcc)
        {
            if (request.ActivationDate is null && request.DueDate is null && request.MoneyAmount is null)
                return Result.Failure<VccIssue>("At least one field must be filled");

            if (request.MoneyAmount is not null && request.MoneyAmount.Value.Currency != vcc.Currency)
                return Result.Failure<VccIssue>("Currency does not match with VCC currency");

            return vcc;
        }


        async Task<Result> UpdateCard((VccIssue, IVccSupplierService) result)
        {
            var (vcc, vccService) = result;

            if (request.MoneyAmount is not null && request.MoneyAmount.Value.Currency != vcc.IssuedCurrency)
                return await _currencyConverterService.ConvertToCurrency(request.MoneyAmount.Value, vcc.IssuedCurrency)
                    .Bind((issuedMoneyAmount) => vccService.Update(vcc, request, issuedMoneyAmount, clientId));

            return await vccService.Update(vcc, request, request.MoneyAmount, clientId);
        }
    }


    private Result<(VccIssue, IVccSupplierService)> GetVccService(VccIssue vcc)
    {
        var (isSuccess, _, service, error) = _serviceResolver.ResolveServiceByVccVendor(vcc.VccVendor);

        return isSuccess
            ? (vcc, service)
            : Result.Failure<(VccIssue, IVccSupplierService)>(error);
    }


    private readonly ILogger<VccService> _logger;
    private readonly VccServiceOptions _options;
    private readonly VccServiceResolver _serviceResolver;
    private readonly IVccIssueRecordsManager _vccRecordsManager;
    private readonly CurrencyConverterService _currencyConverterService;
}