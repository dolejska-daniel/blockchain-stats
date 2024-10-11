using BlockchainStats.App.Export;
using BlockchainStats.App.Logic;
using BlockchainStats.App.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.RPC;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<BitcoinClientOptions>(opts
    => builder.Configuration.GetSection(BitcoinClientOptions.SectionName).Bind(opts));

builder.Services.AddSingleton<IStatsExporter, CsvStatsExporter>();
builder.Services.AddSingleton<RPCClient>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<BitcoinClientOptions>>().Value;
    var credentials = RPCCredentialString.Parse(options.Credentials);
    return new RPCClient(credentials, options.Host, Network.Main);
});

builder.Services.AddHostedService<CollectBitcoinStatsHostedService>();

var app = builder.Build();

await app.RunAsync();
