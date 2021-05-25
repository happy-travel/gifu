using System;
using HappyTravel.Money.Enums;

namespace HappyTravel.Gifu.Data.Models
{
    public class Issue
    {
        public string TransactionId { get; set; } = string.Empty;
        public string ReferenceCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public Currencies Currency { get; set; }
        public DateTime DueDate { get; set; }
    }
}