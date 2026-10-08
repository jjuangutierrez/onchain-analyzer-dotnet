using System.Text.Json.Serialization;

namespace OnChainAnalyzer.Infrastructure;

public record ChainStats(
    [property: JsonPropertyName("funded_txo_sum")] long FundedSum,
    [property: JsonPropertyName("spent_txo_sum")] long SpentSum
);