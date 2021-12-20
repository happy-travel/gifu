using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Money.Models;

namespace HappyTravel.Gifu.Api.Models
{
    public readonly struct VccIssueRequest
    {
        public string ReferenceCode { get; init; }
        public MoneyAmount MoneyAmount { get; init; }        
        public CreditCardTypes? Type { get; init; }
        public DateTime ActivationDate { get; init; }
        public DateTime DueDate { get; init; }
        public Dictionary<string, string?> SpecialValues { get; init; }
    }
}