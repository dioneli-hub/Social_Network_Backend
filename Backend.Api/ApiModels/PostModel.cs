namespace Backend.Api.ApiModels
{
    public class PostModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public SimpleUserModel Author { get; set; } 
    }
}
