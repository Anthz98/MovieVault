using EntityFramework.Configuration;
using EntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

namespace EntityFramework.Context
{
    public class MongoDbContext : DbContext
    {
        private readonly MongoDbSettings mongoDbSettings1;
        public MongoDbContext(DbContextOptions<MongoDbContext> options, MongoDbSettings mongoDbSettings) : base(options)
        {
            mongoDbSettings1 = mongoDbSettings;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Movies>().ToCollection(mongoDbSettings1.MoviesCollection);
            modelBuilder.Entity<Accounts>().ToCollection(mongoDbSettings1.AccountsCollection);
        }

        public DbSet<Movies> Movies { get; set; }
        public DbSet<Accounts> Accounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMongoDB(mongoDbSettings1.ConnectionString, mongoDbSettings1.DatabaseName);
            }
        }

    }
}
