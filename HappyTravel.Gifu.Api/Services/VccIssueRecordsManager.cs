using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Data;
using HappyTravel.Gifu.Data.Models;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Gifu.Api.Services;

public class VccIssueRecordsManager : IVccIssueRecordsManager
{
    public VccIssueRecordsManager(GifuContext context)
    {
        _context = context;
    }


    public Task Add(VccIssue vccIssue)
    {
        _context.VccIssues.Add(vccIssue);
        return _context.SaveChangesAsync();
    }


    public async Task<Result<VccIssue>> Get(string referenceCode)
    {
        var issue = await _context.VccIssues
            .Where(i => i.Status == VccStatuses.Issued)
            .SingleOrDefaultAsync(i => i.ReferenceCode == referenceCode);

        return issue ?? Result.Failure<VccIssue>($"VCC with reference code `{referenceCode}` not found");
    }

        
    public async Task<List<VccIssue>> Get(List<string> referenceCodes)
    {
        return await _context.VccIssues
            .Where(c => referenceCodes.Contains(c.ReferenceCode) && c.Status == VccStatuses.Issued)
            .ToListAsync();
    }


    public Task Remove(VccIssue vccIssue)
    {
        vccIssue.Modified = DateTimeOffset.UtcNow;
        vccIssue.Status = VccStatuses.Deleted;
        _context.Update(vccIssue);
        return _context.SaveChangesAsync();
    }


    public Task Update(VccIssue vccIssue, VccEditRequest changes, MoneyAmount? issuedMoneyAmount)
    {
        var now = DateTimeOffset.UtcNow;
        vccIssue.Modified = now;
                
        if (changes.MoneyAmount is not null) 
            vccIssue.Amount = changes.MoneyAmount.Value.Amount;

        if(issuedMoneyAmount is not null)
            vccIssue.IssuedAmount = issuedMoneyAmount.Value.Amount;

        if (changes.ActivationDate is not null) 
            vccIssue.ActivationDate = changes.ActivationDate.Value;

        if (changes.DueDate is not null) 
            vccIssue.DueDate = changes.DueDate.Value;

        _context.VccDirectEditLogs.Add(new VccDirectEditLog
        {
            VccId = vccIssue.UniqueId,
            Payload = JsonSerializer.Serialize(changes),
            Created = now
        });
                
        _context.Update(vccIssue);
        return _context.SaveChangesAsync();
    }


    public Task DecreaseAmount(VccIssue vccIssue, decimal amount, decimal issuedAmount)
    {
        var now = DateTimeOffset.UtcNow;
        var amountBefore = vccIssue.Amount;
        vccIssue.Amount = amount;
        vccIssue.IssuedAmount = issuedAmount;
        vccIssue.Modified = now;
                
        _context.AmountChangesHistories.Add(new AmountChangesHistory
        {
            VccId = vccIssue.UniqueId,
            AmountAfter = amount,
            AmountBefore = amountBefore,
            Date = now
        });

        _context.Update(vccIssue);
        return _context.SaveChangesAsync();
    }


    public Task<bool> IsIssued(string referenceCode) 
        => _context.VccIssues
            .AnyAsync(vcc => vcc.ReferenceCode == referenceCode && vcc.Status == VccStatuses.Issued);


    private readonly GifuContext _context;
}