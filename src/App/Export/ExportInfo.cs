using BlockchainStats.App.Options;

namespace BlockchainStats.App.Export;

public record ExportInfo(ulong BlockHeightFrom, ulong BlockHeightTo)
{
    public static ExportInfo Create(BlockchainTargetOptions.BlockRange blockRange) => new(blockRange.HeightFrom, blockRange.HeightTo);
}
