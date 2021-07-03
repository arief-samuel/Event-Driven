using UserService.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserService.Data
{
    public class UserServiceContext : DbContext
    {
        public virtual DbSet<User> User { get; set; }
        public UserServiceContext(DbContextOptions<UserServiceContext> options) : base(options)
        {

        }

    }
}