using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Gifu.Api.Infrastructure.Logging;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Data.Models;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HappyTravel.Gifu.Api.Services.VccServices
{
    public class VccService : IVccService
    {
        public VccService(ILogger<VccService> logger, IVccIssueRecordsManager vccRecordsManager, 
            VccServiceResolver serviceResolver)
        {
            _logger = logger;
            _vccRecordsManager = vccRecordsManager;
            _serviceResolver = serviceResolver;
        }


        public async Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, string clientId, CancellationToken cancellationToken)
        {
            _logger.LogVccIssueRequestStarted(request.ReferenceCode, request.MoneyAmount.Amount, request.MoneyAmount.Currency.ToString());

            return await ValidateRequest()
                .Bind(() => _serviceResolver.ResolveServiceByCurrency(request.MoneyAmount.Currency))
                .Bind(vccService => vccService.Issue(request, clientId, cancellationToken));

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
        }


        public async Task<List<VccIssue>> GetCardsInfo(List<string> referenceCodes, CancellationToken cancellationToken)
            => (await _vccRecordsManager.Get(referenceCodes)).TrimCardNumbers().ToList();


        public async Task<Result> Delete(string referenceCode)
        {
            _logger.LogVccDeleteRequestStarted(referenceCode);

            return await _vccRecordsManager.Get(referenceCode)
                .Bind(GetVccService)
                .Bind(DeleteCard);
            

            Task<Result> DeleteCard((VccIssue Vcc, IVccSupplierService vccService) result)
                => result.vccService.Delete(result.Vcc);
        }


        public async Task<Result> ModifyAmount(string referenceCode, MoneyAmount amount)
        {
            _logger.LogVccModifyAmountRequestStarted(referenceCode, amount.Amount);

            return await _vccRecordsManager.Get(referenceCode)
                .Bind(ValidateRequest)
                .Bind(GetVccService)
                .Bind(ModifyCardAmount);

            Result<VccIssue> ValidateRequest(VccIssue vcc)
            {
                if (vcc.Currency != amount.Currency)
                    return Result.Failure<VccIssue>("Amount currency must be equal with VCC currency");

                if (amount.Amount >= vcc.Amount)
                    return Result.Failure<VccIssue>("Amount must be less than VCC amount");

                return vcc;
            }


            Task<Result> ModifyCardAmount((VccIssue Vcc, IVccSupplierService vccService) result)
                => result.vccService.ModifyAmount(result.Vcc, amount);
        }


        public async Task<Result> Edit(string referenceCode, VccEditRequest request, string clientId)
        {
            return await _vccRecordsManager.Get(referenceCode)
                .Bind(GetVccService)
                .Bind(EditCard);


            Task<Result> EditCard((VccIssue Vcc, IVccSupplierService vccService) result)
                => result.vccService.Edit(result.Vcc, request, clientId);
        }


        private Result<(VccIssue, IVccSupplierService)> GetVccService(VccIssue vcc)
        {
            var (isSuccess, _, service, error) = _serviceResolver.ResolveServiceBySupplierCode(vcc.Supplier);

            return isSuccess
                ? (vcc, service)
                : Result.Failure<(VccIssue, IVccSupplierService)>(error);
        }


        private readonly ILogger<VccService> _logger;
        private readonly VccServiceResolver _serviceResolver;
        private readonly IVccIssueRecordsManager _vccRecordsManager;
    }
}
