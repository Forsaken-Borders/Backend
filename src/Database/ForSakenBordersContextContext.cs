using Microsoft.EntityFrameworkCore;

namespace ForSakenBorders.Database
{
    public class ForSakenBordersContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Note> Notes { get; set; }

        public ForSakenBordersContext(DbContextOptions<ForSakenBordersContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<Note>().HasKey(n => n.Id);
            modelBuilder.Entity<Role>().HasKey(r => r.Id);
        }
    }
}