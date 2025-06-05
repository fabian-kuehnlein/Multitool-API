using AutoMapper;
using MultitoolApi.Businesslogic.Models;
using MultitoolApi.DataAccessLayer.Models;
using MultitoolApi.Webapi.Models;
using MultitoolApi.WebApi.Models;

namespace MultitoolApi.ConfigModels;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CalendarEventDTO, CalendarEvent>().ReverseMap();
        CreateMap<CalendarEvent, CalendarEventDAO>().ReverseMap();
        CreateMap<CreateCalendarEventDTO, CreateCalendarEvent>().ReverseMap();
        CreateMap<CreateCalendarEvent, CreateCalendarEventDAO>().ReverseMap();
        CreateMap<Holiday, HolidayDAO>().ReverseMap();
    }
}
