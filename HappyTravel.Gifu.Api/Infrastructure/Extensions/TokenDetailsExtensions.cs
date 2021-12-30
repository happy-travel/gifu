using System;
using System.Globalization;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Models.AmEx.Response;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions;

public static class TokenDetailsExtensions
{
    public static VirtualCreditCard ToVirtualCreditCard(this TokenDetails tokenDetails)
    {
        return new(number: tokenDetails.TokenNumber,
            expiry: DateTime.ParseExact(tokenDetails.TokenExpiryDate, "yyyyMM", CultureInfo.InvariantCulture),
            holder: string.Empty, // AmEx doesn't return cardholder name
            code: tokenDetails.TokenSecurityCode,
            type: CreditCardTypes.AmericanExpress);
    }
}