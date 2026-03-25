using System.Globalization;
using Mapster;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Entities.CustomTable;
using MultitoolApi.WebApi.Models;
using MultitoolApi.WebApi.Models.CustomTable;

namespace Multitool.Application.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateCalendarEvent, CalendarEvent>();

        config.NewConfig<CreateTableDto, Table>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.CreatedAt, src => DateTime.UtcNow);

        config.NewConfig<CreateTableDto, Column>()
            .Map(dest => dest.Name, src => src.Column.Name)
            .Map(dest => dest.DataType, src => src.Column.DataType)
            .Map(dest => dest.ColOrder, src => 0);
    }
}