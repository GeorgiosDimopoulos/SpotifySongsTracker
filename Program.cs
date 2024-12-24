using SpotifySongsTracker.Services;

class Program
{
    static async Task Main(string [] args)
    {
        Console.WriteLine("Hello! Let's see something random on Spotify.");
        SpotifyService spotifyService = new();
        await spotifyService.GetNonUserData();

        Console.WriteLine("\nNow! Let's see what's new in your Spotify account.");
        SpotifyAuth spotifyAuth = new();
        //await spotifyAuth.GetUserData();

        Console.WriteLine("\nProcess completed.");
    }
}
