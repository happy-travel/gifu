using System;
using HappyTravel.Money.Models;

namespace HappyTravel.Gifu.Api.Models;

public struct VccEditRequest
{
    public DateTimeOffset? ActivationDate { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public MoneyAmount? MoneyAmount { get; init; }
}