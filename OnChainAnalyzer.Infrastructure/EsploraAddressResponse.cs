using System.Text.Json.Serialization;

namespace OnChainAnalyzer.Infrastructure;

public record EsploraAddressResponse([property: JsonPropertyName("chain_stats")] ChainStats ChainStats);
