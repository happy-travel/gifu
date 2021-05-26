using System;
using System.Globalization;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Models.AmEx.Response;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions
{
    public static class TokenDetailsExtensions
    {
        public static Vcc ToVcc(this TokenDetails tokenDetails)
        {
            return new ()
            {
                Holder = string.Empty, // AmEx doesn't return cardholder name
                Number = tokenDetails.TokenNumber,
                Code = tokenDetails.TokenSecurityCode,
                Expiry = DateTime.ParseExact(tokenDetails.TokenExpiryDate, "yyyyMM", CultureInfo.InvariantCulture)
            };
        }
    }
}