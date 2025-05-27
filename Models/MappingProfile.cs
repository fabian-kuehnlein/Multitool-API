using AutoMapper;
using CalendarApi.Businesslogic.Models;
using CalendarApi.DataAccessLayer.Models;
using CalendarApi.Webapi.Models;
using CalendarApi.WebApi.Models;

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
