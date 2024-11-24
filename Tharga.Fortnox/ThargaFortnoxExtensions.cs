using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tharga.Fortnox.Internals;

namespace Tharga.Fortnox;

public static class ThargaFortnoxExtensions
{
    public static IServiceCollection AddThargaFortnox(this IServiceCollection services, Action<Options> options = null)
    {
        var fortnoxConfiguration = new Options();
        options?.Invoke(fortnoxConfiguration);

        services.AddHttpClient(Constants.ClientName, x => x.BaseAddress = new Uri(Constants.Root));
        services.AddSingleton(_ => fortnoxConfiguration);
        services.AddTransient<IFortnoxConnectionService>(serviceProvider => new FortnoxConnectionService(serviceProvider.GetService<IConfiguration>(), serviceProvider.GetService<IHttpClientFactory>(), serviceProvider.GetService<Options>()));
        return services;
    }
}