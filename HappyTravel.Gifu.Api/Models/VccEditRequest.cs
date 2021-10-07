using System;
using HappyTravel.Money.Models;

namespace HappyTravel.Gifu.Api.Models
{
    public struct VccEditRequest
    {
        public DateTime? ActivationDate { get; init; }
        public DateTime? DueDate { get; init; }
        public MoneyAmount? MoneyAmount { get; init; }
    }
}