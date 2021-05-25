﻿using HappyTravel.Gifu.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Gifu.Data
{
    public class GifuContext : DbContext
    {
        public GifuContext(DbContextOptions<GifuContext> options) : base(options)
        { }
        
        
        public DbSet<Issue> Issues { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Issue>(b =>
            {
                b.HasKey(i => i.TransactionId);
            });
        }
    }
}