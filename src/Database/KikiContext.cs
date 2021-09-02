using Microsoft.EntityFrameworkCore;

namespace Kiki.Database
{
    public class KikiContext : DbContext
    {
        public KikiContext(DbContextOptions<KikiContext> options) : base(options) { }
    }
}