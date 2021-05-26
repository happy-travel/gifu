using System;
using System.Threading.Tasks;
using HappyTravel.Gifu.Api.Models.AmEx.Response;
using HappyTravel.Money.Models;

namespace HappyTravel.Gifu.Api.Services
{
    public interface IAmExClient
    {
        Task<(string TransactionId, CreateTokenResponse Response)> CreateToken(string referenceCode, MoneyAmount moneyAmount, DateTime dueDate);
    }
}