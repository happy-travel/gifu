using System.Globalization;
using HappyTravel.Money.Models;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions
{
    public static class MoneyAmountExtensions
    {
        public static string ToAmExFormat(this MoneyAmount moneyAmount)
            => (moneyAmount.Amount * 100).ToString(CultureInfo.InvariantCulture);
    }
}