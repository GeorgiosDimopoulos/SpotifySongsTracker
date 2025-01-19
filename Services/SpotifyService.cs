using SpotifyAPI.Web;
using SpotifySongsTracker.Entities;

namespace SpotifySongsTracker.Services;

public class SpotifyService
{
    public async Task GetNonUserData(SpotifyClient spotifyClient)
    {
        Console.WriteLine("\nFetching details of a public track:");
        await GetPublicTrack(spotifyClient, "79QLNktJsGNRz4ijFnDywD");

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
    }

    public async Task GetPublicTrack(SpotifyClient spotifyClient, string trackId)
    {
        try
        {
            var track = await spotifyClient.Tracks.Get(trackId);
            Console.WriteLine($"Track Name: {track.Name}");
            Console.WriteLine($"Album: {track.Album.Name}");
            Console.WriteLine($"Duration: {track.DurationMs / 1000}s");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching track: {ex.Message}");
        }
    }

    public async Task ShowUserPlaylists(SpotifyClient spotifyClient)
    {
        try
        {
            if (spotifyClient?.Playlists == null)
            {
                Console.WriteLine("Spotify client or his Playlists not initialized");
                return;
            }

            if (spotifyClient.Player == null)
            {
                Console.WriteLine("Current user not found");
                return;
            }

            var playlists = await spotifyClient.Playlists.CurrentUsers();
            if (playlists.Items == null || !playlists.Items.Any())
            {
                Console.WriteLine("No playlists found");
                return;
            }

            Console.WriteLine("All of my Playlists:");
            foreach (var playlist in playlists.Items)
            {
                Console.WriteLine($"Playlist: {playlist.Name}, ID: {playlist.Id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching playlists: {ex.Message}");
            return;
        }       
    }

    public async Task ShowUserTopTracks(SpotifyClient spotifyClient)
    {
        Console.WriteLine("\nFetching now user's top tracks...");

        var topTracks = await spotifyClient.Personalization.GetTopTracks();
        if (topTracks.Items == null || topTracks.Items.Count == 0)
        {
            Console.WriteLine("No top tracks found.");
        }
        
        Console.WriteLine("My Top tracks:");
        foreach (var track in topTracks.Items!)
        {
            Console.WriteLine($"Track: {track.Name}, Album: {track.Album.Name}");
        }

        return;
    }
}
