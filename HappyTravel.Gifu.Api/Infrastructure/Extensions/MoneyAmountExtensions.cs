using System;
using System.Globalization;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions
{
    public static class MoneyAmountExtensions
    {
        public static string ToAmExFormat(this MoneyAmount moneyAmount)
        {
            moneyAmount = MoneyRounder.Ceil(moneyAmount);
            return moneyAmount.ToFractionalUnits().ToString(CultureInfo.InvariantCulture);
        }
    }
}