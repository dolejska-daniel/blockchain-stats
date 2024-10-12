using System.Runtime.CompilerServices;
using BlockchainStats.App.Export;
using BlockchainStats.App.Models;
using BlockchainStats.App.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.RPC;

namespace BlockchainStats.App.Logic;

public class CollectBitcoinStatsHostedService(
    RPCClient client,
    IStatsExporter statsExporter,
    IHostApplicationLifetime applicationLifetime,
    IOptions<BlockchainTargetOptions> targetOptions,
    IOptions<BitcoinTransactionStatisticsOptions> processOptions,
    ILogger<CollectBitcoinStatsHostedService> logger
) : BackgroundService
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private CancellationToken CancellationToken => _cancellationTokenSource.Token;
    private BitcoinTransactionStatisticsOptions ProcessOptions => processOptions.Value;
    private BlockchainTargetOptions TargetOptions => targetOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => _cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3)));

        var blockchainInfo = await client.GetBlockchainInfoAsync(CancellationToken);
        logger.LogInformation("Current best block height: {BlockHeight}", blockchainInfo.Blocks);

        var targetRange = TargetOptions.CreateRange(blockchainInfo);
        var transactionStats = GenerateTransactionStatsAsync(targetRange);

        var exportInfo = ExportInfo.Create(targetRange);
        await statsExporter.ExportAsync(exportInfo, transactionStats, stoppingToken);
    }

    private async IAsyncEnumerable<BitcoinTransactionStats> GenerateTransactionStatsAsync(
        BlockchainTargetOptions.BlockRange targetRange)
    {
        logger.LogDebug("Starting process for blocks: {BlockHeightFrom}-{BlockHeightTo}", targetRange.HeightFrom, targetRange.HeightTo);
        var fetchReferences = GetBlockHeightsFrom(targetRange)
            .Select(StartBlockFetch);

        var blocksProcessed = 0u;
        using var fetchReferenceEnumerator = fetchReferences.GetEnumerator();
        var runningReferences = new List<BlockFetchReference>();
        while (true)
        {
            while (runningReferences.Count < ProcessOptions.BlockBatchSize && fetchReferenceEnumerator.MoveNext())
            {
                var blockFetchReference = fetchReferenceEnumerator.Current;
                logger.LogDebug("Fetching block height: {BlockHeight}", blockFetchReference.BlockHeight);
                runningReferences.Add(blockFetchReference);
            }

            if (runningReferences.Count == 0)
            {
                // there are no additional fetch references
                break;
            }

            await Task.WhenAny(runningReferences.Select(x => x.FetchTask));
            var completedReferences = runningReferences.Where(reference => reference.FetchTask.IsCompleted).ToHashSet();
            foreach (var blockFetchReference in completedReferences)
            {
                logger.LogDebug("Finalizing fetch of block at height: {BlockHeight}", blockFetchReference.BlockHeight);
                var block = await blockFetchReference;
                foreach (var transactionStatistic in ProcessBlock(block))
                {
                    yield return transactionStatistic;
                }
                
                var progressPercentage = 100.0 * ++blocksProcessed / targetRange.BlockCount;
                logger.LogInformation("Current progress {BlocksProcessed}/{BlockCount} = {Progress:F1}%", blocksProcessed, targetRange.BlockCount, progressPercentage);
            }

            runningReferences.RemoveAll(reference => completedReferences.Contains(reference));
        }

        applicationLifetime.StopApplication();
        yield break;

        BlockFetchReference StartBlockFetch(uint blockHeight)
            => new(blockHeight, FetchBlockByHeight(blockHeight));

        IEnumerable<BitcoinTransactionStats> ProcessBlock(Block block)
            => block.Transactions.Select(BitcoinTransactionStats.Create);
    }

    async Task<Block> FetchBlockByHeight(uint blockHeight)
    {
        logger.LogDebug("Fetching block hash for height {BlockHeight}", blockHeight);
        var blockHash = await client.GetBlockHashAsync(blockHeight, CancellationToken);

        logger.LogDebug("Fetching block for {BlockHeight} ({BlockHash})", blockHeight, blockHash);
        var response = await client.GetBlockAsync(blockHash, GetBlockVerbosity.WithFullTx, CancellationToken);
        return response.Block ?? throw new ApplicationException($"Block {blockHeight} ({blockHash}) could not be fetched.");
    }

    private static IEnumerable<uint> GetBlockHeightsFrom(BlockchainTargetOptions.BlockRange blockRange)
    {
        for (var blockHeight = blockRange.HeightFrom; blockHeight < blockRange.HeightTo; blockHeight++)
        {
            yield return (uint) blockHeight;
        }
    }

    private record BlockFetchReference(uint BlockHeight, Task<Block> FetchTask)
    {
        public TaskAwaiter<Block> GetAwaiter() => FetchTask.GetAwaiter();
    }
}
