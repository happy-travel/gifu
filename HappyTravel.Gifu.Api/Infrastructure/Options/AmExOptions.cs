using System.Collections.Generic;
using HappyTravel.Gifu.Api.Models.AmEx;

namespace HappyTravel.Gifu.Api.Infrastructure.Options
{
    public class AmExOptions
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public Dictionary<AmexCurrencies, string> Accounts { get; set; } = new();
    }
}