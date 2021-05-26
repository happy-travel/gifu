using System;
using System.Globalization;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Models.AmEx.Response;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions
{
    public static class TokenDetailsExtensions
    {
        public static VccInfo ToVccInfo(this TokenDetails tokenDetails)
        {
            return new ()
            {
                Holder = string.Empty,
                Number = tokenDetails.TokenNumber,
                Code = tokenDetails.TokenSecurityCode,
                Expiry = DateTime.ParseExact(tokenDetails.TokenExpiryDate, "yyyyMM", CultureInfo.InvariantCulture)
            };
        }
    }
}