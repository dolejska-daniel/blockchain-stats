namespace BlockchainStats.App.Options;

public class BitcoinClientOptions
{
    public const string SectionName = "BitcoinClient";

    public string Credentials { get; set; } = string.Empty;
    public Uri Host { get; set; } = new("http://localhost:5000");
}
