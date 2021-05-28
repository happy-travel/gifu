using HappyTravel.Gifu.Api.Infrastructure.Extensions;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Xunit;

namespace HappyTravel.Gifu.Tests
{
    public class MoneyAmountExtensionsTests
    {
        [Theory]
        [InlineData(0.001, Currencies.USD, "0")]
        [InlineData(0.01, Currencies.USD, "1")]
        [InlineData(0.1, Currencies.USD, "10")]
        [InlineData(1, Currencies.USD, "100")]
        [InlineData(1.01, Currencies.USD, "101")]
        [InlineData(1.1, Currencies.USD, "110")]
        [InlineData(1.11, Currencies.USD, "111")]
        [InlineData(1.111, Currencies.USD, "111")]
        [InlineData(1.1111, Currencies.USD, "111")]
        private void Convert_to_amex_format_should_be_equal(decimal amount, Currencies currency, string result)
        {
            var moneyAmount = new MoneyAmount(amount, currency);
            var value = moneyAmount.ToAmExFormat();
            
            Assert.Equal(result, value);
        }
    }
}