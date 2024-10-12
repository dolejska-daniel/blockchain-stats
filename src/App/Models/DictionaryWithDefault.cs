namespace BlockchainStats.App.Models;

public class DictionaryWithDefault<TKey, TValue>(TValue defaultValue) : Dictionary<TKey, TValue>
    where TKey : notnull
{
    public TValue DefaultValue { get; set; } = defaultValue;

    public new TValue this[TKey key]
    {
        get => TryGetValue(key, out var existingValue) ? existingValue : DefaultValue;
        set => base[key] = value;
    }
}
