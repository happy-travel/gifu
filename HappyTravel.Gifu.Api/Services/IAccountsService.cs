using CSharpFunctionalExtensions;
using HappyTravel.Money.Enums;

namespace HappyTravel.Gifu.Api.Services
{
    public interface IAccountsService
    {
        Result<string> GetAccountId(Currencies currency);
    }
}