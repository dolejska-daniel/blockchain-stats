using BlockchainStats.App.Models;

namespace BlockchainStats.App.Export;

public interface IStatsExporter
{
    Task ExportAsync(IEnumerable<BitcoinTransactionStats> stats);
}
