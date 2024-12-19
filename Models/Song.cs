namespace SpotifySongsTracker.Models;

public class Song
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AlbumName { get; set; }
    public int Duration { get; set; }

    //public string ArtistName { get; set; } = string.Empty;
}