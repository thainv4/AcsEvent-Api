using System.Text.Json.Serialization;

namespace AcsEvent.DTOs.AcsEvent;

public class GetAcsEventRequestDto
{
    // ID thiết bị trong database thay vì AuthInfo
    public int ThietBiId { get; set; }
    
    // Request body cho API
    [JsonPropertyName("AcsEventCond")]
    public AcsEventCondition AcsEventCond { get; set; } = new();

}