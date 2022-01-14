using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Data;
using HappyTravel.Gifu.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HappyTravel.Gifu.Api.Services.SupplierServices.IxarisServices;

public class ScheduleLoadRecordsManager : IScheduleLoadRecordsManager
{
    public ScheduleLoadRecordsManager(GifuContext context)
    {
        _context = context;
    }


    public Task Add(IxarisScheduleLoad ixarisScheduleLoad)
    {
        _context.IxarisScheduleLoads.Add(ixarisScheduleLoad);
        return _context.SaveChangesAsync();
    }


    public async Task<Result<IxarisScheduleLoad>> Get(string cardReference)
    {
        var ixarisScheduleLoad = await _context.IxarisScheduleLoads
            .SingleOrDefaultAsync(i => i.CardReference == cardReference);

        return ixarisScheduleLoad ?? Result.Failure<IxarisScheduleLoad>($"`ScheduleLoad` operation with card reference `{cardReference}` not found");
    }


    public Task SetCancelled(IxarisScheduleLoad ixarisScheduleLoad)
    {
        ixarisScheduleLoad.Modified = DateTimeOffset.UtcNow;
        ixarisScheduleLoad.Status = IxarisScheduleLoadStatuses.Canceled;
        _context.Update(ixarisScheduleLoad);
        return _context.SaveChangesAsync();
    }


    private readonly GifuContext _context;
}