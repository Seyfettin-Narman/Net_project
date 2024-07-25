using Microsoft.EntityFrameworkCore;
using innova_project.Models;
namespace innova_project.Data
{
    public class MasrafTakipContext : DbContext
    {
        public MasrafTakipContext(DbContextOptions<MasrafTakipContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ExpenseSummary> ExpenseSummaries { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasMany(u => u.Transactions).WithOne(t => t.User);
        }
    }
}
