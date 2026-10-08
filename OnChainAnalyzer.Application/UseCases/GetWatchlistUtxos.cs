using OnChainAnalyzer.Application.Interfaces;
using OnChainAnalyzer.Domain.ValueObjects;

namespace OnChainAnalyzer.Application.UseCases;

public class GetWatchlistUtxos
{
    private readonly IUtxoProvider _utxoProvider;
    private readonly TimeSpan _delayBetweenRequests;

    public GetWatchlistUtxos(IUtxoProvider utxoProvider, TimeSpan? delayBetweenRequests = null)
    {
        _utxoProvider = utxoProvider;
        _delayBetweenRequests = delayBetweenRequests ?? TimeSpan.FromMilliseconds(350);
    }

    public async Task<IReadOnlyList<TrackedAddressReport>> ExecuteAsync(
        IReadOnlyList<TrackedAddress> targets,
        CancellationToken ct = default)
    {
        var reports = new List<TrackedAddressReport>();
        int total = targets.Count;

        for (int i = 0; i < total; i++)
        {
            ct.ThrowIfCancellationRequested();

            var target = targets[i];
            Console.WriteLine($"[{i + 1}/{total}] Consultando {target.Label} ({target.Address})...");

            try
            {
                var utxos = await _utxoProvider.GetUtxosAsync(target.Address, ct);
                var totalAmount = BitcoinAmount.Sum(utxos.Select(utxo => utxo.Amount));
                reports.Add(new TrackedAddressReport(target, new AddressUtxos(utxos, totalAmount)));
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[OMITIDA] {target.Label} tiene demasiados UTXOs (>500) para el endpoint público de Mempool.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] No se pudo consultar {target.Label}: {ex.Message}");
                Console.ResetColor();
            }

            if (i < total - 1)
            {
                await Task.Delay(_delayBetweenRequests, ct);
            }
        }

        return reports;
    }
}