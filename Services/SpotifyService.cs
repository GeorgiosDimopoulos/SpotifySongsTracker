using SpotifyAPI.Web;
using SpotifySongsTracker.Entities;
using SpotifySongsTracker.Services.Interfaces;

namespace SpotifySongsTracker.Services;

public class SpotifyService : ISpotifyService
{
    private readonly SpotifyAuth _spotifyAuth;

    public SpotifyService()
    {
        _spotifyAuth = new();
    }

    public async Task GetNonUserData()
    {
        var config = SpotifyClientConfig.CreateDefault();
        var clientRequest = new ClientCredentialsRequest(AppResources.ClientId, AppResources.ClientSecret);
        var clientResponse = await new OAuthClient(config).RequestToken(clientRequest);
        var spotifyClient = new SpotifyClient(clientResponse.AccessToken);
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

        FullPlaylist specificPlaylist = await spotifyClient.Playlists.Get("4agdH8mVuilkTYMWxlU0o5");
        if (specificPlaylist == null)
        {
            Console.WriteLine("Playlist not found");
        }

        if (specificPlaylist == null)
        {
            return;
        }
        var playlistModel = new Playlist
        {
            Name = specificPlaylist.Name ?? string.Empty,
            Genre = specificPlaylist.Description,
            Songs = specificPlaylist.Tracks!.Items!.Select(x => new Song
            {
                Name = ((FullTrack)x.Track).Name,
                AlbumName = ((FullTrack)x.Track).Album.Name,
                Duration = ((FullTrack)x.Track).DurationMs
            }).ToList()
        };

        Console.WriteLine($"My Playlist: {playlistModel.Name}");
        foreach (var s in playlistModel.Songs)
        {
            Console.WriteLine($"Song: {s.Name}, Album: {s.AlbumName}, Duration: {s.Duration}");
        }

        //Console.WriteLine("All of my Playlists:");
        //await ShowAllPlaylists(spotifyClient);
    }

    public async Task ShowAllPlaylists(SpotifyClient spotifyClient)
    {
        var playlists = await spotifyClient.Playlists.CurrentUsers();
        if (playlists.Items == null || !playlists.Items.Any())
        {
            Console.WriteLine("No playlists found");
            return;
        }

        foreach (var playlist in playlists.Items)
        {
            Console.WriteLine($"Playlist: {playlist.Name}, ID: {playlist.Id}");
        }
    }
}
