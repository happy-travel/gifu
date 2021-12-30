using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models.Ixaris.Request;
using HappyTravel.Gifu.Api.Models.Ixaris.Response;
using System.Threading.Tasks;

namespace HappyTravel.Gifu.Api.Services.SupplierClients;

public interface IIxarisClient
{
    Task<Result> CancelScheduleLoad(string securityToken, string scheduleReference);
    Task<Result<VccDetails>> GetVirtualCardDetails(string securityToken, string cardReference);
    Task<Result<IssueVcc>> IssueVirtualCard(string securityToken, string virtualCardFactoryName, IssueVccRequest issueVccRequest);
    Task<Result<string>> Login();
    Task<Result<string>> RemoveVirtualCard(string securityToken, string cardReference);
    Task<Result<string>> ScheduleLoad(string securityToken, ScheduleLoadRequest scheduleLoadRequest);
    Task<Result<string>> UpdateScheduleLoad(string securityToken, string scheduleReference, UpdateScheduleLoadRequest updateScheduleLoadRequest);
}