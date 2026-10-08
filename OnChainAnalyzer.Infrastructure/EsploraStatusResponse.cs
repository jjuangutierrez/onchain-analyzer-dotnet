using System.Text.Json.Serialization;

namespace OnChainAnalyzer.Infrastructure;

public record EsploraStatusResponse
{
    [JsonPropertyName("confirmed")] public bool Confirmed { get; init; } = false;
    [JsonPropertyName("block_height")] public int? BlockHeight { get; init; }
    [JsonPropertyName("block_time")] public long? BlockTime { get; init; }
}