using HappyTravel.Gifu.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Gifu.Data;

public class GifuContext : DbContext
{
    public GifuContext(DbContextOptions<GifuContext> options) : base(options)
    { }


    public DbSet<VccIssue> VccIssues => Set<VccIssue>();
    public DbSet<AmountChangesHistory> AmountChangesHistories => Set<AmountChangesHistory>();
    public DbSet<VccDirectEditLog> VccDirectEditLogs => Set<VccDirectEditLog>();
    public DbSet<IxarisScheduleLoad> IxarisScheduleLoads => Set<IxarisScheduleLoad>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VccIssue>(b =>
        {
            b.HasKey(i => i.TransactionId);
        });

        modelBuilder.Entity<AmountChangesHistory>(b =>
        {
            b.HasKey(h => h.Id);
        });

        modelBuilder.Entity<VccDirectEditLog>(b =>
        {
            b.HasKey(l => l.Id);
            b.HasIndex(l => l.VccId);
        });
    }
}