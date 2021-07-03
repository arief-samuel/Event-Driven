namespace PostService.Entities
{
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int UserId { get; set; }

        public string User { get; set; }

    }
}