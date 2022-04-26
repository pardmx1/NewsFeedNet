using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewsFeedNet.Models;

namespace NewsFeedNet.Data
{
    public class NewsFeedDbContext : DbContext
    {
        public NewsFeedDbContext(DbContextOptions<NewsFeedDbContext> options) : base(options)
        {

        }

        public DbSet<PushInfo> PushInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PushInfo>().ToTable("PushInfo");

        }

    }
}
