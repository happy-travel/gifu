using System;
using HappyTravel.Money.Enums;

namespace HappyTravel.Gifu.Data.Models;

public class VccIssue
{
    public string TransactionId { get; set; } = string.Empty;
    public string UniqueId { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Currencies Currency { get; set; }
    public decimal IssuedAmount { get; set; }
    public Currencies IssuedCurrency { get; set; }
    public DateTimeOffset ActivationDate { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Modified { get; set; }
    public VccStatuses Status { get; set; }
    public VccVendors VccVendor { get; init; }
}