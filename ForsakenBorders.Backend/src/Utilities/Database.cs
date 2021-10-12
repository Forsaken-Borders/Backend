using System;
using System.Collections.Generic;
using System.Text.Json;
using ForsakenBorders.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ForsakenBorders.Backend.Utilities
{
    /// <summary>
    /// Accesses the database.
    /// </summary>
    public class BackendContext : DbContext
    {
        /// <summary>
        /// The list of users available for the application.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// A list of roles available to everyone. To get a user's specific roles, use the User.Roles property.
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Logs of all the http requests made to the api.
        /// </summary>
        public DbSet<Log> Logs { get; set; }

        /// <summary>
        /// Creates a new connection to the database.
        /// </summary>
        /// <param name="options">Options to past to the database.</param>
        public BackendContext(DbContextOptions<BackendContext> options) : base(options) { }

        /// <inheritdoc />
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
            modelBuilder.Entity<Role>().HasKey(role => role.Id);
            modelBuilder.Entity<Log>().HasKey(log => log.Id);
            modelBuilder.Entity<Log>().Property(log => log.Exception).HasConversion(exceptionValueConverter);
        }
    }
}