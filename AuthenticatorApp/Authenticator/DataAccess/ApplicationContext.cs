using Authenticator.Models;
using Microsoft.EntityFrameworkCore;

namespace Authenticator.DataAccess
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Step> Steps { get; set; } 
    }
}
