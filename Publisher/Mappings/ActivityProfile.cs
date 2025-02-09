using AutoMapper;
using Publisher.Dtos;
using Publisher.Entities;

namespace Publisher.Mappings;

public class ActivityProfile : Profile
{
    public ActivityProfile()
    {
        CreateMap<CreateActivityDto, Activity>().ReverseMap();
        CreateMap<UpdateActivityDto, Activity>().ReverseMap();
    }
}