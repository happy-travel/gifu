using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models;

namespace HappyTravel.Gifu.Api.Services
{
    public interface IVccService
    {
        Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, CancellationToken cancellationToken);
    }
}