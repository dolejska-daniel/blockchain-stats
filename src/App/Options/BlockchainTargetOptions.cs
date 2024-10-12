using NBitcoin.RPC;

namespace BlockchainStats.App.Options;

public record BlockchainTargetOptions : IOptions
{
    public string SectionName => "BlockchainTarget";

    protected ulong? BlockHeightFrom { get; set; }
    protected ulong? BlockHeightTo { get; set; }
    protected ulong? BlockCount { get; set; }

    public BlockRange CreateRange(BlockchainInfo blockchainInfo) => this switch
    {
        {BlockHeightFrom: not null, BlockHeightTo: not null} => new BlockRange(BlockHeightFrom.Value, BlockHeightTo.Value),
        {BlockHeightFrom: not null, BlockCount: not null} => new BlockRange(BlockHeightFrom.Value, BlockHeightFrom.Value + BlockCount.Value),
        {BlockHeightTo: not null, BlockCount: not null} => new BlockRange(BlockHeightTo.Value - BlockCount.Value, BlockHeightTo.Value),
        {BlockHeightFrom: null, BlockHeightTo: null, BlockCount: not null} => new BlockRange(blockchainInfo.Blocks - BlockCount.Value, blockchainInfo.Blocks),
        _ => throw new ApplicationException($"Export option misconfigured! Could not calculate requested block heights from: {this}"),
    };

    public record BlockRange(ulong HeightFrom, ulong HeightTo)
    {
        public double BlockCount => HeightTo - HeightFrom;
    }
}
