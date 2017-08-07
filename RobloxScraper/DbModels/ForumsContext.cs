using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobloxScraper.DbModels
{
    public class ForumsContext : DbContext
    {
        public ForumsContext() : base() { ChangeTracker.AutoDetectChangesEnabled = false; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {         
            optionsBuilder.UseSqlite("Data Source=forums.db");
        }

        public DbSet<ForumGroup> ForumGroups { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<Thread> Threads { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
