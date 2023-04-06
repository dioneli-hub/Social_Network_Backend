namespace Backend.Api.ApiModels
{
    public class PostCommentModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // info about author
    }
}
