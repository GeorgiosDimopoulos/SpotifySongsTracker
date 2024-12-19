namespace SpotifySongsTracker.Models;

public class Playlist
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<Song> Songs { get; set; } = default!;
    public string? Genre { get; set; }
}
