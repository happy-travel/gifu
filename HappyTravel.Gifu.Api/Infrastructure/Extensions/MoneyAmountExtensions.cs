using System;
using System.Globalization;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Models;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions
{
    public static class MoneyAmountExtensions
    {
        public static string ToAmExFormat(this MoneyAmount moneyAmount)
        {
            return Math.Ceiling(moneyAmount.Amount * (int)Math.Pow(10, moneyAmount.Currency.GetDecimalDigitsCount()))
                .ToString(CultureInfo.InvariantCulture);
        }
    }
}