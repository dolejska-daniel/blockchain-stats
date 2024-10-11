using NBitcoin;

namespace BlockchainStats.App.Models;

public class BitcoinTransactionStats
{
    public required string TxId { get; init; }
    public uint Version { get; init; }

    public bool HasHeightLock { get; init; }
    public bool HasNonZeroHeightLock { get; init; }
    public uint HeightLock { get; init; }
    public bool HasTimeLock { get; init; }

    public bool HasHeightLockEnabled { get; init; }
    public bool HasInputWithFeeSnipping { get; init; }
    public bool HasInputWithRBF { get; init; }

    public int InputCount { get; init; }
    public int OutputCount { get; init; }

    public static BitcoinTransactionStats Create(Transaction tx) => new()
    {
        TxId = tx.GetHash().ToString(),
        Version = tx.Version,
        HasNonZeroHeightLock = tx.LockTime is { IsHeightLock: true, Value: > 0 },
        HasHeightLock = tx.LockTime.IsHeightLock,
        HeightLock = tx.LockTime.Value,
        HasTimeLock = tx.LockTime.IsTimeLock,
        HasHeightLockEnabled = tx.Inputs.Any(input => input.Sequence < Sequence.Final),
        HasInputWithRBF = tx.Inputs.Any(input => input.Sequence.IsRBF),
        HasInputWithFeeSnipping = tx.Inputs.Any(input => input.Sequence == Sequence.FeeSnipping),
        InputCount = tx.Inputs.Count,
        OutputCount = tx.Outputs.Count,
    };
}
