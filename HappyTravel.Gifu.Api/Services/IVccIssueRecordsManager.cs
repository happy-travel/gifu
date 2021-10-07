using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Data.Models;

namespace HappyTravel.Gifu.Api.Services
{
    public interface IVccIssueRecordsManager
    {
        Task Add(VccIssue vccIssue);
        Task<Result<VccIssue>> Get(string referenceCode);
        Task<List<VccIssue>> Get(List<string> referenceCodes);
        Task<bool> IsIssued(string referenceCode);
        Task Delete(VccIssue vccIssue);
        Task Edit(VccIssue vccIssue, VccEditRequest changes);
        Task ModifyAmount(VccIssue vccIssue, decimal amount);
    }
}