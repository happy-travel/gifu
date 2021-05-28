using System.Threading.Tasks;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Gifu.Api.Models.AmEx.Response;

namespace HappyTravel.Gifu.Api.Services
{
    public interface IAmExClient
    {
        Task<(string TransactionId, CreateTokenResponse Response)> CreateToken(CreateTokenRequest payload);
    }
}