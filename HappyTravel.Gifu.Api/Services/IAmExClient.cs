using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Api.Models.AmEx.Response;

namespace HappyTravel.Gifu.Api.Services
{
    public interface IAmExClient
    {
        Task<Result<(string TransactionId, AmexResponse Response)>> CreateToken(CreateTokenRequest payload);
        Task<Result<(string TransactionId, AmexResponse Response)>> Delete(DeleteRequest payload);
        Task<Result<(string TransactionId, AmexResponse Response)>> ModifyAmount(ModifyRequest payload);
    }
}