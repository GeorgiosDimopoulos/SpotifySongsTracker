using SpotifySongsTracker.Entities;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace SpotifySongsTracker.Services;

public class ExportsService
{
    public static void ExportToCsv(List<Artist> artists, List<Song> songs, List<Playlist> playlists)
    {
        // Ensure arguments are not null
        artists ??= new List<Artist>();
        songs ??= new List<Song>();
        playlists ??= new List<Playlist>();

        var exportsDir = Path.Combine(Directory.GetCurrentDirectory(), "Exports");
        if (!Directory.Exists(exportsDir))
            Directory.CreateDirectory(exportsDir);

        // Helper to escape CSV fields
        static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var mustQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            var escaped = value.Replace("\"", "\"\"");
            return mustQuote ? '"' + escaped + '"' : escaped;
        }

        // --- Songs CSV ---
        var songsPath = Path.Combine(exportsDir, "songs.csv");
        var sb = new StringBuilder();
        sb.AppendLine("ArtistName,SongName,AlbumName,Duration");

        foreach (var song in songs)
        {
            var artistName = artists.FirstOrDefault(a => a.Songs != null && a.Songs.Any(s => s.Id == song.Id))?.Name ?? string.Empty;
            var line = string.Join(",",
                EscapeCsv(artistName),
                EscapeCsv(song.Name),
                EscapeCsv(song.AlbumName),
                song.Duration.ToString());

            sb.AppendLine(line);
        }

        File.WriteAllText(songsPath, sb.ToString(), Encoding.UTF8);

        // --- Artists CSV ---
        var artistsPath = Path.Combine(exportsDir, "artists.csv");
        sb.Clear();
        sb.AppendLine("ArtistName,NumberOfSongs");

        foreach (var artist in artists)
        {
            var count = artist.Songs?.Count ?? 0;
            var line = string.Join(",",
                EscapeCsv(artist.Name),
                count.ToString());
            sb.AppendLine(line);
        }

        File.WriteAllText(artistsPath, sb.ToString(), Encoding.UTF8);

        // --- Playlists CSV ---
        var playlistsPath = Path.Combine(exportsDir, "playlists.csv");
        sb.Clear();
        sb.AppendLine("PlaylistName,NumberOfSongs,Genre");

        foreach (var playlist in playlists)
        {
            var count = playlist.Songs?.Count ?? 0;
            var line = string.Join(",",
                EscapeCsv(playlist.Name),
                count.ToString(),
                EscapeCsv(playlist.Genre));
            sb.AppendLine(line);
        }

        File.WriteAllText(playlistsPath, sb.ToString(), Encoding.UTF8);
    }
}
