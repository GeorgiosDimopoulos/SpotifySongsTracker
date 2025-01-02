using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpotifySongsTracker.Services;

class Program
{
    static async Task Main(string [] args)
    {
        Console.WriteLine("Hello! Let's see something random on Spotify.");

        // Step 1: Create a ServiceCollection and configure services
        Console.WriteLine("\nFirst, let's load the required services");
        var services = new ServiceCollection();
        ConfigureServices(services);

        // Step 2: Build the ServiceProvider
        var serviceProvider = services.BuildServiceProvider();

        // Step 3: Get the auth service and register with own credentials
        var spotifyAuth = serviceProvider.GetRequiredService<SpotifyAuth>();
        await spotifyAuth.AuthenticateUserAsync();

        // Step 4: Get the main service and run your app logic
        var spotifyService = serviceProvider.GetRequiredService<SpotifyService>();
        await spotifyService.GetNonUserData();

        Console.WriteLine("\nProcess completed.");
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<SpotifyAuth>();
        services.AddScoped<SpotifyService>();

        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });
    }
}
