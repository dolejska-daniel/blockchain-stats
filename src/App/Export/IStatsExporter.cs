using BlockchainStats.App.Models;

namespace BlockchainStats.App.Export;

public interface IStatsExporter
{
    Task ExportAsync(ExportInfo info, IAsyncEnumerable<BitcoinTransactionStats> stats, CancellationToken cancellationToken = default);
}
