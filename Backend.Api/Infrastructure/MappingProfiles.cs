using AutoMapper;
using Backend.ApiModels;
using Backend.Domain;

namespace Backend.Api.Infrastructure
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<PostComment, PostCommentModel>();
            CreateMap<User , SimpleUserModel>();
            CreateMap<User, UserModel>()
                .ForMember(dest => dest.TotalFollowers, opt => opt.MapFrom(src => src.UserFollowers.Count()))
                .ForMember(dest => dest.TotalFollowsTo, opt => opt.MapFrom(src => src.UserFollowsTo.Count()));
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
