using Microsoft.AspNetCore.SignalR;
using SpotifySongsTracker.Dashboard.Services;

namespace SpotifySongsTracker.Dashboard.Hubs;

/// <summary>
/// SignalR hub that streams Spotify stats to connected dashboard clients in real-time.
/// Clients invoke <see cref="RequestStats"/> to trigger a fetch; the server pushes each
/// data section (top tracks, top genres, top playlists) back as it becomes available
/// instead of waiting for all data before responding.
/// </summary>
public class StatsHub : Hub
{
    private readonly SpotifyStatsService _statsService;

    public StatsHub(SpotifyStatsService statsService)
    {
        _statsService = statsService;
    }

    /// <summary>
    /// Called by the client to start streaming the authenticated user's Spotify stats.
    /// Results are broadcast back to the calling connection via the following client methods:
    /// <list type="bullet">
    ///   <item><c>ReceiveTopTracks</c> — list of top-track objects</item>
    ///   <item><c>ReceiveTopGenres</c> — list of genre/count objects</item>
    ///   <item><c>ReceiveTopPlaylists</c> — list of playlist objects</item>
    ///   <item><c>StatsError</c> — error message string if something goes wrong</item>
    ///   <item><c>StatsComplete</c> — sent with no payload once all sections are done</item>
    /// </list>
    /// </summary>
    public async Task RequestStats()
    {
        await _statsService.StreamStatsToClient(Clients.Caller);
    }
}
