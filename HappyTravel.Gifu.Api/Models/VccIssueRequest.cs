using System;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Money.Models;

namespace HappyTravel.Gifu.Api.Models
{
    public readonly struct VccIssueRequest
    {
        [Required]
        public string ReferenceCode { get; init; }
        
        [Required]
        public MoneyAmount MoneyAmount { get; init; }
        
        [Required]
        public DateTime DueDate { get; init; }
    }
}