using HappyTravel.Gifu.Api.Models;
using System.Collections.Generic;

namespace HappyTravel.Gifu.Api.Infrastructure.Options;

public class IxarisOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public Dictionary<CreditCardTypes, string> VccFactoryNames { get; set; } = new();
    public CreditCardTypes DefaultVccType { get; set; }
}