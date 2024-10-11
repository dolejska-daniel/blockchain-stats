using BlockchainStats.App.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlockchainStats.App.Extensions;

public static class HostBuilderExtensions
{
    public static IHostApplicationBuilder ConfigureOptions<T>(this IHostApplicationBuilder hostBuilder)
        where T : class, IOptions
    {
        hostBuilder.Services.Configure<T>(options => hostBuilder.Configuration.GetSection(options.SectionName).Bind(options));
        return hostBuilder;
    }
}
