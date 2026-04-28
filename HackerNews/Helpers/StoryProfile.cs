using AutoMapper;
using Hacker_News.Contracts;
namespace Hacker_News.Helpers
{
    public class StoryProfile : Profile
    {
        public StoryProfile()
        {
            CreateMap<StoryApiClientResponse, Story>()
                .ForMember(d => d.CommentsCount, m => m.MapFrom(s => s.Kids.Count()))
                .ForMember(d => d.Uri, m => m.MapFrom(s => s.Url))
                .ForMember(d => d.PostedBy, m => m.MapFrom(s => s.By));
        }
    }
}
