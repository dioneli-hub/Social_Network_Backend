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
            CreateMap<User , SimpleUserModel>();
            CreateMap<User, UserModel>();
            CreateMap<Post, PostModel>();
            CreateMap<PostComment, PostCommentModel>();
            CreateMap<PostLike, PostLikeModel>();
            CreateMap<Post, PostModel>()
                .ForMember(dest => dest.TotalLikes, opt => opt.MapFrom(src => src.Likes.Count()))
                .ForMember(dest => dest.TotalComments, opt => opt.MapFrom(src => src.Comments.Count())); ;
            CreateMap<ApplicationFile, ApplicationFileModel>();

        }
    }
}
