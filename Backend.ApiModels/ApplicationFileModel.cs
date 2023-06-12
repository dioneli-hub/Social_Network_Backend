namespace Backend.ApiModels
{
    public class ApplicationFileModel
    {
        public int Id { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
