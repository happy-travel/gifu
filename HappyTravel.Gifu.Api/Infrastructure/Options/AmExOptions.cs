using System.Collections.Generic;
using HappyTravel.Money.Enums;

namespace HappyTravel.Gifu.Api.Infrastructure.Options;

public class AmExOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public Dictionary<Currencies, string> Accounts { get; set; } = new();
}