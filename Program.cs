using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        // get public data
        var spotifyClient = await spotifyAuth.InitializeSpotifyClient();
        
        if (spotifyClient is null)
        {
            Console.WriteLine("Spotify client could not be initialized.");
            return;
        }

        await spotifyService.GetNonUserData(spotifyClient);

        // get private user data
        var authenticatedUser = await spotifyAuth.AuthenticateUserAsync();
        await spotifyService.ShowUserTopTracks(authenticatedUser);
        await spotifyService.ShowUserPlaylists(authenticatedUser);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SpotifyClient>();
        services.AddScoped<SpotifyAuth>();
        services.AddScoped<SpotifyService>();

        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });
    }
}
