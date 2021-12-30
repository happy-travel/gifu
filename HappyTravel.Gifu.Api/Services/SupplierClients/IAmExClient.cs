using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Api.Models.AmEx.Response;

namespace HappyTravel.Gifu.Api.Services.SupplierClients;

public interface IAmExClient
{
    Task<Result<(string TransactionId, TokenIssuanceData Response)>> CreateToken(CreateTokenRequest payload);
    Task<Result<(string TransactionId, TokenIssuanceData Response)>> Remove(DeleteRequest payload);
    Task<Result<(string TransactionId, TokenIssuanceData Response)>> Update(ModifyRequest payload);
}