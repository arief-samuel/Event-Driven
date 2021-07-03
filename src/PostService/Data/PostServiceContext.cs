using Microsoft.EntityFrameworkCore;

namespace PostService.Data
{
    public class PostServiceContext : DbContext
    {
        public PostServiceContext(DbContextOptions<PostServiceContext> options)
            : base(options)
        {
        }

        public DbSet<PostService.Entities.Post> Post { get; set; }
        public DbSet<PostService.Entities.User> User { get; set; }
    }
}