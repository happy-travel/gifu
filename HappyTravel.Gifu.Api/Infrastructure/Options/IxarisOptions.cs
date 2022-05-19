using HappyTravel.Gifu.Api.Models;
using HappyTravel.Money.Enums;
using System.Collections.Generic;

namespace HappyTravel.Gifu.Api.Infrastructure.Options;

public class IxarisOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Dictionary<Currencies, string> Accounts { get; set; } = new();
    public Dictionary<CreditCardTypes, string> VccFactoryNames { get; set; } = new();
    public CreditCardTypes DefaultVccType { get; set; }
}