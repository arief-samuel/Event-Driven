using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PostService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PostService.Data
{
    public class DataAccess
    {
        private readonly List<string> _connectionStrings = new List<string>();

        public DataAccess(IConfiguration configuration)
        {
            var connectionStrings = configuration.GetSection("PostDbConnectionStrings");
            foreach (var connectionString in connectionStrings.GetChildren())
            {
                Console.WriteLine("ConnectionString: " + connectionString.Value);
                _connectionStrings.Add(connectionString.Value);
            }
        }

        public async Task<ActionResult<IEnumerable<Post>>> ReadLatestPosts(string category, int count)
        {
            using var dbContext = new PostServiceContext(GetConnectionString(category));
            return await dbContext.Post.OrderByDescending(p => p.PostId)
                                        .Take(count)
                                        .Include(x => x.User)
                                        .Where(p => p.CategoryId == category)
                                        .ToListAsync();
        }

        public async Task<int> CreatePost(Post post)
        {
            using var dbContext = new PostServiceContext(GetConnectionString(post.CategoryId));
            dbContext.Post.Add(post);
            return await dbContext.SaveChangesAsync();
        }

        public async Task InitDatabase(int countUsers, int countCategories)
        {
            foreach (var connectionString in _connectionStrings)
            {
                using var dbContext = new PostServiceContext(connectionString);
                await dbContext.Database.EnsureDeletedAsync();
                await dbContext.Database.EnsureDeletedAsync();
                for (int i = 1; i <= countUsers; i++)
                {
                    await dbContext.User.AddAsync(new User { Name = "User" + i, Version = 1 });
                    await dbContext.SaveChangesAsync();
                }
                for (int i = 1; i <= countCategories; i++)
                {
                    await dbContext.Category.AddAsync(new Category { CategoryId = "Category" + i });
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private string GetConnectionString(string category)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(category));
            var x = BitConverter.ToUInt16(hash, 0) % _connectionStrings.Count;
            return _connectionStrings[x];
        }
    }
}