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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VccIssue>(b =>
            {
                b.HasKey(i => i.TransactionId);
            });

            modelBuilder.Entity<AmountChangesHistory>(b =>
            {
                b.HasKey(h => h.VccId);
            });
        }
    }
}