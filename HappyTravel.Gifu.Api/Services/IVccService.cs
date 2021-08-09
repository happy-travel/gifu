using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Data.Models;

namespace HappyTravel.Gifu.Api.Services
{
    public interface IVccService
    {
        Task<Result<VirtualCreditCard>> Issue(VccIssueRequest request, string clientId, CancellationToken cancellationToken);
        Task<List<VccIssue>> GetCardsInfo(List<string> referenceCodes, CancellationToken cancellationToken);
    }
}