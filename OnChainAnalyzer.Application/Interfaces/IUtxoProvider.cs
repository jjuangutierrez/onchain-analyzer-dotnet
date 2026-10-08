using OnChainAnalyzer.Domain.Entities;

namespace OnChainAnalyzer.Application.Interfaces;

public interface IUtxoProvider
{
    Task<IReadOnlyList<Utxo>> GetUtxosAsync(string address, CancellationToken c = default);
}