namespace Multitool.Application.Models.Kanban;

public record ColumnModel(
    long Id,
    string Name,
    int Position,
    List<CardModel> Cards);
