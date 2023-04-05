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
        }
    }
}
