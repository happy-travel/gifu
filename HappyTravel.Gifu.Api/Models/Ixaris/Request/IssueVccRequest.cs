using HappyTravel.Money.Enums;
using System.Collections.Generic;

namespace HappyTravel.Gifu.Api.Models.Ixaris.Request;

public readonly struct IssueVccRequest
{
    public Currencies Currency { get; init; }

    public string? FundingAccountReference { get; init; }

    public decimal? Amount { get; init; }

    public List<Dictionary<string, string>> CardInfo { get; init; }
}