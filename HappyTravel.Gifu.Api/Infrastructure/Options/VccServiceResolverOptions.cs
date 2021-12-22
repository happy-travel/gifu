using HappyTravel.Money.Enums;
using System.Collections.Generic;

namespace HappyTravel.Gifu.Api.Infrastructure.Options
{
    public class VccServiceResolverOptions
    {
        public List<Currencies> AmexCurrencies { get; set; } = new();
        public List<Currencies> IxarisCurrencies { get; set; } = new();
    }
}
