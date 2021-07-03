using Microsoft.EntityFrameworkCore;
using UserService.Entities;

namespace UserService.Data
{
    public class UserServiceContext : DbContext
    {
        public UserServiceContext(DbContextOptions<UserServiceContext> options)
            : base(options)
        {
        }

        public DbSet<User> User { get; set; }
    }
}