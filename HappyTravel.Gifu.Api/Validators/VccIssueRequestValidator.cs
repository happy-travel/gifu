using System;
using FluentValidation;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Services;

namespace HappyTravel.Gifu.Api.Validators;

public class VccIssueRequestValidator : AbstractValidator<VccIssueRequest>
{
    public VccIssueRequestValidator(IVccIssueRecordsManager vccIssueRecordsManager)
    {
        var today = DateTimeOffset.UtcNow.Date;
        
        RuleFor(r => r.ActivationDate.Date).GreaterThanOrEqualTo(today);
        RuleFor(r => r.DueDate.Date).GreaterThan(r => r.ActivationDate.Date);
        RuleFor(r => r.MoneyAmount.Amount).GreaterThan(0);
        RuleFor(r => r.ReferenceCode)
            .NotEmpty()
            .MustAsync(async (referenceCode, _) => !await vccIssueRecordsManager.IsIssued(referenceCode))
            .WithMessage("A VCC for '{PropertyValue}' was already issued");
    }
}