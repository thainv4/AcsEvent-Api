using System.Text.Json.Serialization;

namespace AcsEvent.DTOs.AcsEvent;

public class AcsEventCondition
{
    [JsonPropertyName("searchId")] 
    public string SearchId { get; set; } = "1";

    [JsonPropertyName("searchResultPosition")]
    public int SearchResultPosition { get; set; } = 0;

    [JsonPropertyName("maxResults")] public int MaxResults { get; set; } = 1000;
    [JsonPropertyName("major")]
    public int Major { get; set; } = 0;
    [JsonPropertyName("minor")]
    public int Minor { get; set; } = 0;
    [JsonPropertyName("startTime")]
    public DateTimeOffset StartTime { get; set; }
    [JsonPropertyName("endTime")]
    public DateTimeOffset EndTime { get; set; }
    [JsonPropertyName("employeeNoString")]
    public string EmployeeNoString { get; set; }
}