using Microsoft.AspNetCore.SignalR;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace SpotifySongsTracker.Dashboard.Services;

/// <summary>
/// Fetches the authenticated user's Spotify stats and streams each section back
/// to the connected SignalR client as soon as it is ready.
/// </summary>
public class SpotifyStatsService
{
    private readonly IConfiguration _config;
    private static EmbedIOAuthServer? _server;
    private static SpotifyClient? _spotifyClient;
    private static readonly SemaphoreSlim _authLock = new(1, 1);

    public SpotifyStatsService(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Authenticates (or reuses an existing client) and then pushes top tracks,
    /// top genres and top playlists to the SignalR caller one section at a time.
    /// </summary>
    public async Task StreamStatsToClient(IClientProxy caller)
    {
        try
        {
            var client = await GetOrCreateClientAsync();

            await PushTopTracksAsync(client, caller);
            await PushTopGenresAsync(client, caller);
            await PushTopPlaylistsAsync(client, caller);

            await caller.SendAsync("StatsComplete");
        }
        catch (Exception ex)
        {
            await caller.SendAsync("StatsError", ex.Message);
        }
    }

    // -------------------------------------------------------------------------
    // Auth helpers
    // -------------------------------------------------------------------------

    private async Task<SpotifyClient> GetOrCreateClientAsync()
    {
        if (_spotifyClient is not null)
            return _spotifyClient;

        await _authLock.WaitAsync();
        try
        {
            if (_spotifyClient is not null)
                return _spotifyClient;

            var clientId = _config["Spotify:ClientId"]
                ?? throw new InvalidOperationException("Spotify:ClientId is not configured.");
            var clientSecret = _config["Spotify:ClientSecret"]
                ?? throw new InvalidOperationException("Spotify:ClientSecret is not configured.");
            var redirectUri = _config["Spotify:RedirectUri"] ?? "http://localhost:5000/callback";
            var port = new Uri(redirectUri).Port;

            var tcs = new TaskCompletionSource<SpotifyClient>();

            _server = new EmbedIOAuthServer(new Uri(redirectUri), port);
            _server.AuthorizationCodeReceived += async (_, response) =>
            {
                await _server.Stop();
                var config = SpotifyClientConfig.CreateDefault();
                var tokenResponse = await new OAuthClient(config).RequestToken(
                    new AuthorizationCodeTokenRequest(clientId, clientSecret, response.Code, new Uri(redirectUri)));
                tcs.TrySetResult(new SpotifyClient(tokenResponse.AccessToken));
            };
            _server.ErrorReceived += async (_, error, _) =>
            {
                await _server.Stop();
                tcs.TrySetException(new Exception($"Spotify auth error: {error}"));
            };

            await _server.Start();

            var loginRequest = new LoginRequest(new Uri(redirectUri), clientId, LoginRequest.ResponseType.Code)
            {
                Scope =
                [
                    Scopes.UserReadEmail,
                    Scopes.UserTopRead,
                    Scopes.PlaylistReadPrivate,
                    Scopes.PlaylistReadCollaborative,
                ]
            };

            BrowserUtil.Open(loginRequest.ToUri());

            _spotifyClient = await tcs.Task;
            return _spotifyClient;
        }
        finally
        {
            _authLock.Release();
        }
    }

    // -------------------------------------------------------------------------
    // Data fetchers — each pushes its section immediately via SignalR
    // -------------------------------------------------------------------------

    private static async Task PushTopTracksAsync(SpotifyClient client, IClientProxy caller)
    {
        var topTracks = await client.Personalization.GetTopTracks(new PersonalizationTopRequest
        {
            Limit = 10,
            TimeRangeParam = PersonalizationTopRequest.TimeRange.MediumTerm
        });

        var tracks = (topTracks.Items ?? [])
            .Select(t => new { t.Name, Album = t.Album.Name })
            .ToList();

        await caller.SendAsync("ReceiveTopTracks", tracks);
    }

    private static async Task PushTopGenresAsync(SpotifyClient client, IClientProxy caller)
    {
        var allArtists = new List<FullArtist>();
        for (int offset = 0; offset < 500; offset += 50)
        {
            var page = await client.Personalization.GetTopArtists(new PersonalizationTopRequest
            {
                Limit = 50,
                TimeRangeParam = PersonalizationTopRequest.TimeRange.LongTerm,
                Offset = offset
            });
            allArtists.AddRange(page.Items ?? []);
        }

        var genreCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var artist in allArtists)
        {
            foreach (var genre in artist.Genres)
            {
                var key = (genre.Equals("laïko", StringComparison.OrdinalIgnoreCase) ||
                           genre.Equals("entehno", StringComparison.OrdinalIgnoreCase))
                    ? "Greek"
                    : genre;

                genreCount[key] = genreCount.GetValueOrDefault(key) + 1;
            }
        }

        var topGenres = genreCount
            .OrderByDescending(kv => kv.Value)
            .Take(10)
            .Select(kv => new { Genre = kv.Key, Count = kv.Value })
            .ToList();

        await caller.SendAsync("ReceiveTopGenres", topGenres);
    }

    private static async Task PushTopPlaylistsAsync(SpotifyClient client, IClientProxy caller)
    {
        var result = await client.Playlists.CurrentUsers();
        var playlists = (result.Items ?? [])
            .OrderByDescending(p => p.Tracks?.Total ?? 0)
            .Take(5)
            .Select(p => new { p.Name, TrackCount = p.Tracks?.Total ?? 0 })
            .ToList();

        await caller.SendAsync("ReceiveTopPlaylists", playlists);
    }
}
