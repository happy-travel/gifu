using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Data.Models;
using HappyTravel.Money.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HappyTravel.Gifu.Api.Services.VccServices;

public interface IVccSupplierService
{
    Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, string clientId, CancellationToken cancellationToken);
    Task<Result> Remove(VccIssue Vcc);
    Task<Result> DecreaseAmount(VccIssue Vcc, MoneyAmount amount);
    Task<Result> Update(VccIssue Vcc, VccEditRequest request, string clientId);

}