﻿using System;
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
            var amount = moneyAmount.Amount * (int) Math.Pow(10, moneyAmount.Currency.GetDecimalDigitsCount());
            return ((int)amount).ToString(CultureInfo.InvariantCulture);
        }
    }
}