using System;
using System.Globalization;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToAmExFormat(this DateTime dateTime)
            => dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
    }
}