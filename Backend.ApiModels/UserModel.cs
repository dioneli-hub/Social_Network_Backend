namespace Backend.ApiModels
{
    public class UserModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public ApplicationFileModel Avatar { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalFollowsTo { get; set; }
    }
}
