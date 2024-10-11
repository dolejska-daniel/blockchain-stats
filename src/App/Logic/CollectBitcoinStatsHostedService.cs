using BlockchainStats.App.Export;
using BlockchainStats.App.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoreLinq;
using NBitcoin;
using NBitcoin.RPC;

namespace BlockchainStats.App.Logic;

public class CollectBitcoinStatsHostedService(
    RPCClient client,
    IStatsExporter statsExporter,
    ILogger<CollectBitcoinStatsHostedService> logger
) : BackgroundService
{
    private const int LastBlockCount = 10;
    private const int BlockBatchSize = 10;

    private readonly List<BitcoinTransactionStats> _transactionStats = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var blockchainInfo = await client.GetBlockchainInfoAsync(stoppingToken);
        var blockTaskBatches = GetLastBlockHeights(blockchainInfo, LastBlockCount)
            .Select(async blockHeight =>
            {
                logger.LogDebug("Fetching block hash for height {BlockHeight}", blockHeight);
                var blockHash = await client.GetBlockHashAsync(blockHeight, stoppingToken);

                logger.LogDebug("Fetching block for {BlockHeight} ({BlockHash})", blockHeight, blockHash);
                var response = await client.GetBlockAsync(blockHash, GetBlockVerbosity.WithFullTx, stoppingToken);
                return response.Block;
            })
            .Batch(BlockBatchSize)
            .Select(Task.WhenAll);

        foreach (var blockBatchTask in blockTaskBatches)
        {
            logger.LogDebug("Processing block batch");
            var blockBatch = await blockBatchTask;
            blockBatch.Flatten().Cast<Block>().ForEach(ProcessBlock);
        }

        logger.LogDebug("Exporting {StatsCount} collected statistics", _transactionStats.Count);
        await statsExporter.ExportAsync(_transactionStats);
    }

    private void ProcessBlock(Block block)
    {
        logger.LogDebug("Processing block from {BlockTime}", block.Header.BlockTime);
        var stats = block.Transactions.Select(BitcoinTransactionStats.Create);
        _transactionStats.AddRange(stats);
    }

    private static IEnumerable<uint> GetLastBlockHeights(BlockchainInfo blockchainInfo, uint blockCount)
    {
        for (var blockHeight = blockchainInfo.Blocks - blockCount; blockHeight < blockchainInfo.Blocks; blockHeight++)
        {
            yield return (uint) blockHeight;
        }
    }
}