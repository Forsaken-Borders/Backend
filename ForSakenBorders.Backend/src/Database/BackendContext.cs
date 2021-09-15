using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace ForSakenBorders.Backend.Database
{
    public class BackendContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Log> Logs { get; set; }

        public BackendContext(DbContextOptions<BackendContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(user => user.Email).IsUnique();
            modelBuilder.Entity<User>().HasKey(user => user.Id);
            modelBuilder.Entity<Note>().HasKey(note => note.Id);
            modelBuilder.Entity<Role>().HasKey(role => role.Id);
            modelBuilder.Entity<Log>().HasKey(log => log.Id);

            if (Startup.Configuration.GetValue<bool>("dev"))
            {
                ValueConverter<List<string>, string> stringValueConverter = new(
                    value => string.Join(",", value),
                    value => new List<string>(value.Split(',', System.StringSplitOptions.RemoveEmptyEntries))
                );

                modelBuilder.Entity<Note>().Property(note => note.Tags).HasConversion(stringValueConverter);
            }
        }
    }
}