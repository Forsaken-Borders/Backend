using Microsoft.EntityFrameworkCore;

namespace Kiki.Database
{
    public class KikiContext : DbContext
    {
        public KikiContext(DbContextOptions<KikiContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Note> Notes { get; set; }
    }
}