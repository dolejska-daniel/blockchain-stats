using System.Globalization;
using BlockchainStats.App.Models;
using CsvHelper;
using Microsoft.Extensions.Logging;

namespace BlockchainStats.App.Export;

public class CsvStatsExporter(ILogger<CsvStatsExporter> logger) : IStatsExporter
{
    public async Task ExportAsync(IEnumerable<BitcoinTransactionStats> stats)
    {
        var exportFile = new FileInfo("stats.csv");

        logger.LogDebug("Creating export file {FilePath}", exportFile.FullName);
        await using var exportFileStream = exportFile.OpenWrite();
        await using var exportFileWriter = new StreamWriter(exportFileStream);
        await using var csvWriter = new CsvWriter(exportFileWriter, CultureInfo.InvariantCulture);
        
        logger.LogInformation("Exporting to {FilePath}", exportFile.FullName);
        await csvWriter.WriteRecordsAsync(stats);
        logger.LogInformation("Exporting finished at {FilePath}", exportFile.FullName);
    }
}
