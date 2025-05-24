using AutoMapper;
using CalendarApi.Businesslogic.Models;
using CalendarApi.DataAccessLayer.Models;
using CalendarApi.WebApi.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateCalendarEventDTO, CreateCalendarEvent>().ReverseMap();
        CreateMap<CreateCalendarEvent, CreateCalendarEventDAO>().ReverseMap();
        // CreateMap<CalendarEventEntity, CalendarEvent>().ReverseMap();

        // Falls du weitere DTOs/Entities hast, hier ergänzen
    }
}
