using SpotifyAPI.Web;
using SpotifySongsTracker;
using SpotifySongsTracker.Models;

Console.WriteLine("Hello! Lets see whats new in your spotify account");

var config = SpotifyClientConfig.CreateDefault();
var request = new ClientCredentialsRequest(AppResources.ClientId, AppResources.ClientSecret);
var response = await new OAuthClient(config).RequestToken(request);

var spotifyClient = new SpotifyClient(response.AccessToken);
Console.WriteLine("Connected to Spotify!");

var track = await spotifyClient.Tracks.Get("79QLNktJsGNRz4ijFnDywD");
Song song = new()
{
    Name = track.Name,
    Id = track.Id,
    AlbumName = track.Album.Name,
    Duration = track.DurationMs
};

Console.WriteLine($"Song: {song.Name}, Album: {song.AlbumName}, Duration: {song.Duration}");

var playlitst = await GetSpecificPlaylist();

await GetSpecificPlaylist();

async Task<Playlist> GetSpecificPlaylist()
{
    var specificPlaylist = await spotifyClient.Playlists.Get("4agdH8mVuilkTYMWxlU0o5");
    if (specificPlaylist == null)
    {
        Console.WriteLine("Playlist not found");
        return null!;
    }

    return new()
    {
        Id = specificPlaylist!.Id ?? "",
        Name = specificPlaylist!.Name ?? "",
        Genre = specificPlaylist!.Description ?? "",
        Songs = new List<Song> { song }
    };
}