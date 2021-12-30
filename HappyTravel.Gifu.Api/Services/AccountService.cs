using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Money.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Gifu.Api.Services;

public class AccountService : IAccountsService
{
    public AccountService(IOptions<AmExOptions> options)
    {
        _options = options.Value;
    }


    public Result<string> GetAccountId(Currencies currency)
    {
        return _options.Accounts.TryGetValue(currency, out var accountId)
            ? accountId
            : Result.Failure<string>($"Cannot get account for currency `{currency}`");
    }
        
        
    private readonly AmExOptions _options;
}