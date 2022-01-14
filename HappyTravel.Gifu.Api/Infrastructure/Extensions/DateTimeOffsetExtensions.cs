using System;
using System.Globalization;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions;

public static class DateTimeOffsetExtensions
{
    public static string ToAmExFormat(this DateTimeOffset dateTimeOffset)
        => dateTimeOffset.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
}