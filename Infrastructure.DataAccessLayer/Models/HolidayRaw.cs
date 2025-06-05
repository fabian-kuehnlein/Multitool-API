using System.Text.Json.Serialization;

namespace MultitoolApi.DataAccessLayer.Models;

public class HolidayRaw
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
    [JsonPropertyName("fname")]
    public string Fname { get; set; } = string.Empty;
}