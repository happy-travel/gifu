using HappyTravel.Money.Enums;
using System.Collections.Generic;

namespace HappyTravel.Gifu.Api.Infrastructure.Options;

public class VccServiceOptions
{
    public Dictionary<Currencies, Currencies> CurrenciesToConvert = new();
}
