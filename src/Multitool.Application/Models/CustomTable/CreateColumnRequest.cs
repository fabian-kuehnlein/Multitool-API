using Multitool.Domain.Enums;

namespace Multitool.Application.Models.CustomTable;

public record CreateColumnRequest(
    string Name,
    CustomDataType DataType,
    int ColOrder
);
