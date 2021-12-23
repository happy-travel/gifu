using CSharpFunctionalExtensions;
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

            return await _serviceResolver.ResolveService(request.Types, request.MoneyAmount.Currency)
                .Bind(vccService => vccService.Issue(request, clientId, cancellationToken));
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


            Task<Result> DecreaseCardAmount((VccIssue Vcc, IVccSupplierService vccService) result)
                => result.vccService.DecreaseAmount(result.Vcc, amount);
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


            Task<Result> UpdateCard((VccIssue Vcc, IVccSupplierService vccService) result)
                => result.vccService.Update(result.Vcc, request, clientId);
        }


        private Result<(VccIssue, IVccSupplierService)> GetVccService(VccIssue vcc)
        {
            var (isSuccess, _, service, error) = _serviceResolver.ResolveServiceByVccVendor(vcc.VccVendor);

            return isSuccess
                ? (vcc, service)
                : Result.Failure<(VccIssue, IVccSupplierService)>(error);
        }


        private readonly ILogger<VccService> _logger;
        private readonly VccServiceResolver _serviceResolver;
        private readonly IVccIssueRecordsManager _vccRecordsManager;
    }
}
