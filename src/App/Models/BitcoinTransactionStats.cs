using NBitcoin;

namespace BlockchainStats.App.Models;

public class BitcoinTransactionStats
{
    public static BitcoinTransactionStats Create(Transaction tx)
    {
        return new BitcoinTransactionStats
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


    public string TxId { get; set; } = string.Empty;
    public uint Version { get; set; }

    public bool HasHeightLock { get; set; }
    public bool HasNonZeroHeightLock { get; set; }
    public uint HeightLock { get; set; }
    public bool HasTimeLock { get; set; }

    public bool HasHeightLockEnabled { get; set; }
    public bool HasInputWithFeeSnipping { get; set; }
    public bool HasInputWithRBF { get; set; }

    public int InputCount { get; set; }
    public int OutputCount { get; set; }
}
