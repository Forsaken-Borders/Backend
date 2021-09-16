using System;
using System.Collections.Generic;
using System.Text.Json;
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
            ValueConverter<List<string>, string> stringListValueConverter = new(
                value => string.Join(",", value),
                value => new List<string>(value.Split(',', StringSplitOptions.RemoveEmptyEntries))
            );

            ValueConverter<Exception, string> exceptionValueConverter = new(
                value => JsonSerializer.Serialize(value, new JsonSerializerOptions()),
                value => JsonSerializer.Deserialize<Exception>(value, new JsonSerializerOptions())
            );

            modelBuilder.Entity<User>().HasIndex(user => user.Email).IsUnique();
            modelBuilder.Entity<User>().HasKey(user => user.Id);
            modelBuilder.Entity<Note>().HasKey(note => note.Id);
            modelBuilder.Entity<Role>().HasKey(role => role.Id);
            modelBuilder.Entity<Log>().HasKey(log => log.Id);
            modelBuilder.Entity<Log>().Property(log => log.Exception).HasConversion(exceptionValueConverter);

            if (Startup.Configuration.GetValue<bool>("dev"))
            {
                modelBuilder.Entity<Note>().Property(note => note.Tags).HasConversion(stringListValueConverter);
            }
        }
    }
}