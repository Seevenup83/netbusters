//Data/DatabaseContext.cs
using Microsoft.EntityFrameworkCore;
using netbusters.Models;

namespace netbusters.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
    }
}