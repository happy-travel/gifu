using HappyTravel.Money.Enums;

namespace HappyTravel.Gifu.Api.Models.Ixaris.Request;

public readonly struct IssueVccRequest
{
    public Currencies Currency { get; init; }
      
    public string? FundingAccountReference { get; init; }

    public decimal? Amount { get; init; }     
}