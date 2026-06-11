using Multitool.Domain.Enums;

namespace Multitool.Application.Models.CustomTable;

public record UpdateColumnDto(
    string Name,
    CustomDataType DataType,
    int ColOrder
);
