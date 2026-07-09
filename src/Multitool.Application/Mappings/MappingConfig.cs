using Mapster;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Entities.CustomTable;
using Multitool.Application.Models.CustomTable;
using Multitool.Application.Models.Calendar;
using Multitool.Application.Models.Info;

namespace Multitool.Application.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CalendarEvent, EventSearchResponseDto>()
            .Map(dest => dest.EventId, src => src.Id)
            .Map(dest => dest.EventTitle, src => src.Title)
            .Map(dest => dest.EventNote, src => src.Note);

        config.NewConfig<CalendarEvent, CalendarEventDto>()
            .Map(dest => dest.Id, src => src.Id.ToString())
            .Map(dest => dest.IsTodo, src => false);

        // -----------------------------------------------------------------

        config.NewConfig<Row, RowInfo>()
            .Map(dest => dest.Cells, src => src.Cells.ToDictionary(
                c => c.ColumnId,
                c => CellValue(c)
            ));

        config.NewConfig<CreateTableDto, Table>()
            .Map(dest => dest.CreatedAt, src => DateTime.Now);
    }

    private static object? CellValue(Cell cell) =>
        cell.ValString is not null ? (object?)cell.ValString :
        cell.ValInt    is not null ? (object?)cell.ValInt    :
        cell.ValDec    is not null ? (object?)cell.ValDec    :
        cell.ValDate   is not null ? (object?)cell.ValDate   :
        cell.ValBool   is not null ? (object?)cell.ValBool   :
        null;
}