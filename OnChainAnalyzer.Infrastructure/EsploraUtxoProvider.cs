using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using OnChainAnalyzer.Application.Interfaces;
using OnChainAnalyzer.Domain.Entities;
using OnChainAnalyzer.Domain.ValueObjects;

namespace OnChainAnalyzer.Infrastructure;

public class EsploraUtxoProvider : IUtxoProvider
{
    private readonly HttpClient _httpClient;
    
    public EsploraUtxoProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static DateTime? ConvertToDate(long? unixTime)
    {
        if (!unixTime.HasValue) return null;
        return DateTimeOffset.FromUnixTimeSeconds(unixTime.Value).UtcDateTime;
    }

    public async Task<IReadOnlyList<Utxo>> GetUtxosAsync(string address, CancellationToken ct = default)
    {
        const int maxRetries = 3;
        int attempts = 0;

        while (true)
        {
            attempts++;
            try
            {
                var responses = await _httpClient.GetFromJsonAsync<List<EsploraUtxoResponse>>(
                    $"address/{address}/utxo", ct) ?? [];
                
                return responses.Select(r => new Utxo(
                    r.Txid, 
                    r.Vout, 
                    BitcoinAmount.FromSatoshis(r.Value), 
                    r.Status.BlockHeight, 
                    ConvertToDate(r.Status.BlockTime)
                )).ToList();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                var stats = await _httpClient.GetFromJsonAsync<EsploraAddressResponse>($"address/{address}", ct);
                if (stats?.ChainStats != null)
                {
                    long totalSats = stats.ChainStats.FundedSum - stats.ChainStats.SpentSum;
                    return [new Utxo("0000000000000000000000000000000000000000000000000000000000000000", 0, BitcoinAmount.FromSatoshis(totalSats), null, null)];
                }
                return [];
            }
            catch (HttpRequestException ex) when ((ex.StatusCode == HttpStatusCode.TooManyRequests || ex.InnerException != null) && attempts <= maxRetries)
            {
                int waitSeconds = attempts * 5;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: attempted again {address[..10]}... in {waitSeconds}s (reason: {ex.StatusCode?.ToString() ?? "connection failed"}).");
                Console.ResetColor();

                await Task.Delay(TimeSpan.FromSeconds(waitSeconds), ct);
            }
        }
    }
}