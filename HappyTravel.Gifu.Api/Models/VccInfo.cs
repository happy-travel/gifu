using System;
using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Gifu.Api.Models
{
    public readonly struct VccInfo
    {
        public string Number { get; init; }
        public DateTime Expiry { get; init; }
        public string Holder { get; init; }
        public string Code { get; init; }
    }
}