using PostService.Entities;
using Microsoft.EntityFrameworkCore;

namespace PostService.Data
{
    public class PostServiceContext : DbContext
    {
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Post> Post { get; set; }
        public PostServiceContext(DbContextOptions<PostServiceContext> options) : base(options)
        {

        }

    }
}