using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotifySongsTracker.Entities;

namespace SpotifySongsTracker.Services;

public class SpotifyService
{
    public async Task ShowUserTopStats(SpotifyClient spotifyClient)
    {
        try
        {

            if (await GetUserTopTracks(spotifyClient))
            {
                await GetUserTopGenres(spotifyClient);
            }

            await GetUserTopPlaylists(spotifyClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching playlists: {ex.Message}");
            return;
        }
    }

    private async Task GetUserTopGenres(SpotifyClient spotifyClient)
    {
        var topArtistsList = new List<FullArtist>();
        for (int i = 0; i < 500; i += 50)
        {
            var currenttopArtists = await spotifyClient.Personalization.GetTopArtists(new PersonalizationTopRequest
            {
                Limit = 50,
                TimeRangeParam = PersonalizationTopRequest.TimeRange.LongTerm,
                Offset = i
            });

            topArtistsList?.AddRange(currenttopArtists.Items?.ToList());
        }

        if (topArtistsList == null || topArtistsList.Count == 0)
        {
            Console.WriteLine("No top artists found.");
            return;
        }
        Console.WriteLine("\nMy Top 10 Artists:");
        foreach (var artist in topArtistsList.Take(10))
        {
            Console.WriteLine($"Artist: {artist.Name}");
        }

        var genreCount = new Dictionary<string, int>();
        foreach (var artist in topArtistsList)
        {
            foreach (var genre in artist.Genres)
            {
                string cgenre = genre;

                if (artist.Genres.Where(g => g.Equals("laïko", StringComparison.OrdinalIgnoreCase) || g.Equals("entehno", StringComparison.OrdinalIgnoreCase)).Any())
                    cgenre = "Greek";

                if (genreCount.ContainsKey(cgenre))
                {
                    genreCount [cgenre]++;
                }
                else
                {
                    genreCount.Add(cgenre, 1);
                }
            }
        }

        var sortedGenres = genreCount.OrderByDescending(x => x.Value);
        Console.WriteLine("\nMy 10 Top Genres:");
        foreach (var genre in sortedGenres.Take(10))
        {
            Console.WriteLine($"Genre: {genre.Key}, Count: {genre.Value}");
        }
    }

    private async Task GetPublicRandomTrack(SpotifyClient spotifyClient, string trackId)
    {
        var track = await spotifyClient.Tracks.Get(trackId);
        Console.WriteLine($"Track Name: {track.Name} in album {track.Album.Name}");
    }

    private static async Task GetUserTopPlaylists(SpotifyClient spotifyClient)
    {
        if (spotifyClient?.Playlists?.CurrentUsers() == null)
        {
            Console.WriteLine("Spotify client or his Playlists not initialized");
            return;
        }

        var playlists = (await spotifyClient.Playlists.CurrentUsers()).Items;
        if (playlists == null || playlists.Count == 0)
        {
            Console.WriteLine("No playlists found");
            return;
        }

        Console.WriteLine("\nMy 5 top Playlists:");

        foreach (var playlist in playlists.OrderByDescending(x => x.Tracks!.Total).Take(5))
        {
            Console.WriteLine($"Playlist: {playlist.Name}, has {playlist.Tracks!.Total} tracks");
        }
    }

    private static async Task<bool> GetUserTopTracks(SpotifyClient spotifyClient)
    {
        var topTracks = await spotifyClient.Personalization.GetTopTracks(new PersonalizationTopRequest()
        {
            Limit = 10,
            TimeRangeParam = PersonalizationTopRequest.TimeRange.MediumTerm
        });

        if (topTracks.Items == null || topTracks.Items.Count == 0)
        {
            Console.WriteLine("No top tracks found.");
            return false;
        }

        Console.WriteLine("\nMy Top 10 tracks the last 6 monhts:");
        foreach (var track in topTracks.Items)
        {
            Console.WriteLine($"Track: {track.Name}, Album: {track.Album.Name}");
        }

        return true;
    }

    private async Task GetNonUserData(SpotifyClient spotifyClient)
    {
        // Console.WriteLine("\nFetching details of a random track:");
        // await GetPublicTrack(spotifyClient, "79QLNktJsGNRz4ijFnDywD");

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
}
