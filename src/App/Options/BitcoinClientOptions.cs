namespace BlockchainStats.App.Options;

public class BitcoinClientOptions : IOptions
{
    public string SectionName => "BitcoinClient";

    public string Credentials { get; set; } = string.Empty;
    public Uri Host { get; set; } = new("http://localhost:5000");
}
