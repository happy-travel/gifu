using System;
using System.Globalization;
using HappyTravel.Money.Models;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions
{
    public static class MoneyAmountExtensions
    {
        public static string ToAmExFormat(this MoneyAmount moneyAmount)
        {
            var (amount, _) = moneyAmount;
            int count = BitConverter.GetBytes(decimal.GetBits(amount)[3])[2];
            return ((int)(amount * (int)Math.Pow(10, count))).ToString(CultureInfo.InvariantCulture);
        }
    }
}