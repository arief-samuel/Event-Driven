using Microsoft.EntityFrameworkCore;

namespace PostService.Data
{
    public class PostServiceContext : DbContext
    {
        private readonly string _connectionString;

        public PostServiceContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(_connectionString);
        }

        public DbSet<PostService.Entities.Post> Post { get; set; }
        public DbSet<PostService.Entities.User> User { get; set; }
        public DbSet<PostService.Entities.Category> Category { get; set; }
    }
}