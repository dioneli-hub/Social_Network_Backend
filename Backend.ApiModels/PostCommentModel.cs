namespace Backend.ApiModels
{
    public class PostCommentModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public SimpleUserModel Author { get; set; }
        public int AuthorId { get; set; }
        public int PostId { get; set; }
    }
}
