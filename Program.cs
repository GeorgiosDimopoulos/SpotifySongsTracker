using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using SpotifyAPI.Web;
using SpotifySongsTracker.Services;

class Program
{
    static async Task Main(string [] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var services = new ServiceCollection();
        ConfigureServices(services);

        var serviceProvider = services.BuildServiceProvider();
        var spotifyService = serviceProvider.GetRequiredService<SpotifyService>();
        var spotifyAuth = serviceProvider.GetRequiredService<SpotifyAuth>();

        var spotifyClient = await spotifyAuth.InitializeSpotifyClient();
        
        if (spotifyClient is null)
        {
            Console.WriteLine("Spotify client could not be initialized.");
            return;
        }

        var authenticatedUser = await spotifyAuth.AuthenticateUserAsync();
        await spotifyService.ShowUserTopStats(authenticatedUser);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SpotifyClient>();
        services.AddScoped<SpotifyAuth>();
        services.AddScoped<SpotifyService>();       
    }
}
