using AutoMapper;
using MultitoolApi.Businesslogic.Models;
using MultitoolApi.WebApi.Models;

namespace MultitoolApi.ConfigModels;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CalendarEventDTO, CalendarEvent>().ReverseMap();
        CreateMap<CreateCalendarEventDTO, CreateCalendarEvent>().ReverseMap();
        CreateMap<EventSearchResponseDTO, EventSearchResponse>().ReverseMap();
    }
}
