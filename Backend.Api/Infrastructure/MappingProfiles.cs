using AutoMapper;
using Backend.Api.ApiModels;
using Backend.DataAccess.Entities;

namespace Backend.Api.Infrastructure
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<PostComment, PostCommentModel>();
            //CreateMap< List<User>, List<SimpleUserModel>> ();
            CreateMap<User , SimpleUserModel>();
            CreateMap<User, UserModel>();
            CreateMap<Post, PostModel>();
            CreateMap<PostComment, PostCommentModel>();
            CreateMap<PostLike, PostLikeModel>();
        }
    }
}
