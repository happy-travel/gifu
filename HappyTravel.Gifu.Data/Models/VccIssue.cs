using System;
using HappyTravel.Money.Enums;

namespace HappyTravel.Gifu.Data.Models
{
    public class VccIssue
    {
        public string TransactionId { get; set; } = string.Empty;
        public string UniqueId { get; set; } = string.Empty;
        public string ReferenceCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public Currencies Currency { get; set; }
        public DateTime ActivationDate { get; set; }
        public DateTime DueDate { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}