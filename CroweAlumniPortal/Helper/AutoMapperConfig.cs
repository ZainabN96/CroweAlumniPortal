using AutoMapper;
using CroweAlumniPortal.Dtos;
using CroweAlumniPortal.Models;

namespace CroweAlumniPortal.Helper
{
    public class AutoMapperConfig
    {
        public static IMapper RegisterMappings()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDto>().ReverseMap();
                cfg.CreateMap<Event, EventDto>().ReverseMap();
                cfg.CreateMap<Post, PostDto>().ReverseMap();
                cfg.CreateMap<User, ProfileDto>()
                      .ForMember(d => d.CurrentCity, o => o.MapFrom(s => s.City))
                      .ForMember(d => d.Designation, o => o.MapFrom(s => s.JobTitle))
                      .ForMember(d => d.EmailAddress, o => o.MapFrom(s => s.EmailAddress))
                      .ReverseMap();
            });

            return config.CreateMapper();
        }

    }
}
