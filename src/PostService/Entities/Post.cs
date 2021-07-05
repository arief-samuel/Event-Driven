using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PostService.Entities
{
    [Index(nameof(PostId), nameof(CategoryId))]
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public string CategoryId { get; set; }
        public Category Category { get; set; }
    }
}