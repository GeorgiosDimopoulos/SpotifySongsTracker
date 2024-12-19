using SpotifyAPI.Web;
using SpotifySongsTracker.Models;

Console.WriteLine("Hello! Lets see whats new in your spotify account");

var config = SpotifyClientConfig.CreateDefault();
var request = new ClientCredentialsRequest("YourClientId", "YourClientSecret");
var response = await new OAuthClient(config).RequestToken(request);

var spotifyClient = new SpotifyClient(response.AccessToken); // "YourAccessToken"
Console.WriteLine("Connected to Spotify!");

var track = await spotifyClient.Tracks.Get("1s6ux0lNiTziSrd7iUAADH");
Song song = new()
{
    Name = track.Name,
    Id = track.Id,
    AlbumName = track.Album.Name,
    Duration = track.DurationMs
};

var playlist = await spotifyClient.Playlists.Get("4agdH8mVuilkTYMWxlU0o5");
Playlist playlist1 = new()
{
    Id = playlist.Id,
    Name = playlist.Name,
    Genre = playlist.Description,
    Songs = new List<Song> { song }
};