using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tharga.Fortnox.Tests;

public class ThargaFortnoxRegistrationTests
{
    [Fact]
    public void AddThargaFortnox_Registers_IFortnoxConnectionService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddThargaFortnox(o =>
        {
            o.ClientId = "id";
            o.ClientSecret = "secret";
            o.RedirectUri = new Uri("https://example.com/callback");
        });

        using var provider = services.BuildServiceProvider();
        var service = provider.GetService<IFortnoxConnectionService>();

        Assert.NotNull(service);
        Assert.IsType<FortnoxConnectionService>(service);
    }

    [Fact]
    public void AddThargaFortnox_Registers_HttpClient_With_Correct_BaseAddress()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddThargaFortnox();

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient(Constants.ClientName);

        Assert.Equal(new Uri(Constants.Root), client.BaseAddress);
    }

    [Fact]
    public void AddThargaFortnox_Without_Options_Still_Registers_Service()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddThargaFortnox();

        using var provider = services.BuildServiceProvider();
        var service = provider.GetService<IFortnoxConnectionService>();

        Assert.NotNull(service);
    }
}
