using Mapster;
using Multitool.Application.Models;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Entities.CustomTable;
using MultitoolApi.WebApi.Models.CustomTable;
namespace Multitool.Application.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CalendarEvent, EventSearchResponse>()
            .Map(dest => dest.EventId, src => src.Id)
            .Map(dest => dest.EventTitle, src => src.Title)
            .Map(dest => dest.EventNote, src => src.Note)
            .Map(dest => dest.StartDateTime, src => src.StartDateTime);
            
        config.NewConfig<CreateCalendarEvent, CalendarEvent>();

        // -----------------------------------------------------------------

        config.NewConfig<Table, TableOverview>();

        config.NewConfig<Table, TableDetail>()
            .Map(dest => dest.Columns, src => src.Columns.Adapt<List<ColumnInfo>>())
            .Map(dest => dest.Rows, src => src.Rows.Adapt<List<RowInfo>>());

        config.NewConfig<Column, ColumnInfo>();

        config.NewConfig<Row, RowInfo>()
            .Map(dest => dest.Cells, src => src.Cells.ToDictionary(
                c => c.ColumnId,
                c => CellValue(c)
            ));

        config.NewConfig<CreateTableDto, Table>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.CreatedAt, src => DateTime.UtcNow);

        config.NewConfig<CreateTableDto, Column>()
            .Map(dest => dest.Name, src => src.Column.Name)
            .Map(dest => dest.DataType, src => src.Column.DataType)
            .Map(dest => dest.ColOrder, src => 0);
    }

    private static object? CellValue(Cell cell) =>
        cell.ValString is not null ? (object?)cell.ValString :
        cell.ValInt    is not null ? (object?)cell.ValInt    :
        cell.ValDec    is not null ? (object?)cell.ValDec    :
        cell.ValDate   is not null ? (object?)cell.ValDate   :
        cell.ValBool   is not null ? (object?)cell.ValBool   :
        null;
}