using System.Text.Json;
using OnChainAnalyzer.Application.UseCases;
using OnChainAnalyzer.Infrastructure;

var json = await File.ReadAllTextAsync("watchlist.json");
var targets = JsonSerializer.Deserialize<List<TrackedAddress>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

using var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://mempool.space/api/")
};

var utxoProvider = new EsploraUtxoProvider(httpClient);
var watchlistUseCase = new GetWatchlistUtxos(utxoProvider, TimeSpan.FromMilliseconds(600));

var reports = await watchlistUseCase.ExecuteAsync(targets);

Console.WriteLine("\n--- FINAL SUMMARY ---");
foreach (var report in reports)
{
    Console.WriteLine($"[{report.Target.Category}] {report.Target.Label}: {report.Details.Total} ({report.Details.Utxos.Count} UTXOs)");
}