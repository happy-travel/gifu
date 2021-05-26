using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Money.Models;

namespace HappyTravel.Gifu.Api.Services
{
    public interface IAmExClient
    {
        Task<Result<VccInfo>> CreateCard(string referenceCode, MoneyAmount moneyAmount, DateTime dueDate);
    }
}