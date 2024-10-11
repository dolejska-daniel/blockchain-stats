using BlockchainStats.App.Models;

namespace BlockchainStats.App.Export;

public interface IStatsExporter
{
    Task ExportAsync(IAsyncEnumerable<BitcoinTransactionStats> stats, CancellationToken cancellationToken = default);
}
