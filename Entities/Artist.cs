namespace SpotifySongsTracker.Entities;

public class Artist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Song> Songs { get; set; } = default!;
}
