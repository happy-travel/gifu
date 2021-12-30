using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Data.Models;
using System.Threading.Tasks;

namespace HappyTravel.Gifu.Api.Services.SupplierServices.IxarisServices;

public interface IScheduleLoadRecordsManager
{
    Task Add(IxarisScheduleLoad ixarisScheduleLoad);
    Task<Result<IxarisScheduleLoad>> Get(string cardReference);
    Task SetCancelled(IxarisScheduleLoad ixarisScheduleLoad);
}