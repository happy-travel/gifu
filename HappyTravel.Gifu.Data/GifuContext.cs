using HappyTravel.Gifu.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Gifu.Data
{
    public class GifuContext : DbContext
    {
        public GifuContext(DbContextOptions<GifuContext> options) : base(options)
        { }


        public DbSet<VccIssue> VccIssues => Set<VccIssue>();
        public DbSet<AmountChangesHistory> AmountChangesHistories => Set<AmountChangesHistory>();
        public DbSet<VccEditLog> VccEditLogs => Set<VccEditLog>();


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

            modelBuilder.Entity<VccEditLog>(b =>
            {
                b.HasKey(l => l.Id);
                b.HasIndex(l => l.VccId);
            });
        }
    }
}