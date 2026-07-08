using System.Text.Json.Serialization;

namespace Multitool.Infrastructure.ApiClients.Models;

public record HolidayRaw(
    [property: JsonPropertyName("date")] string Date,
    [property: JsonPropertyName("fname")] string Fname
);

public record HolidayResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("feiertage")] List<HolidayRaw> Feiertage
);
