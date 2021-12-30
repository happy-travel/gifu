using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Data.Models;
using HappyTravel.Money.Models;

namespace HappyTravel.Gifu.Api.Services.VccServices;

public interface IVccService
{   
    Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, string clientId, CancellationToken cancellationToken);
    Task<List<VccIssue>> GetCardsInfo(List<string> referenceCodes, CancellationToken cancellationToken);
    Task<Result> Remove(string referenceCode);
    Task<Result> DecreaseAmount(string referenceCode, MoneyAmount amount);
    Task<Result> Update(string referenceCode, VccEditRequest request, string clientId);
}