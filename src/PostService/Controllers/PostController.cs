using Microsoft.AspNetCore.Mvc;
using PostService.Data;
using PostService.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly DataAccess _dataAccess;

        public PostsController(DataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetLatestPosts(string category, int count)
        {
            return await _dataAccess.ReadLatestPosts(category, count);
        }

        [HttpPost]
        public async Task<ActionResult<Post>> PostingPost(Post post)
        {
            await _dataAccess.CreatePost(post);
            return NoContent();
        }

        [HttpGet("InitDatabase")]
        public async Task InitDatabase([FromQuery] int countUsers, [FromQuery] int countCategories)
        {
            await _dataAccess.InitDatabase(countUsers, countCategories);
            await Task.CompletedTask;
        }
    }
}