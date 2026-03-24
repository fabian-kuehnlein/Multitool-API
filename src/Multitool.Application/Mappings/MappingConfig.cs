using System.Globalization;
using Mapster;
using Multitool.Domain.Entities.Calendar;
using MultitoolApi.WebApi.Models;

namespace Multitool.Application.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateCalendarEvent, CalendarEvent>();
    }
}