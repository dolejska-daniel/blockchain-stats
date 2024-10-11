namespace BlockchainStats.App.Options;

public class BitcoinTransactionStatisticsOptions : IOptions
{
    public string SectionName => "BitcoinTransactionStatistics";

    public uint LastBlockCount { get; set; } = 10;
    public short BlockBatchSize { get; set; } = 5;
}
