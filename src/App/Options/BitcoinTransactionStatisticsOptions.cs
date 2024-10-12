namespace BlockchainStats.App.Options;

public class BitcoinTransactionStatisticsOptions : IOptions
{
    public string SectionName => "BitcoinTransactionStatistics";

    public short BlockBatchSize { get; set; } = 5;
}
