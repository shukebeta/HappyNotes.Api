namespace HappyNotes.Models.Search;

public class ManticoreHits
{
    public long total { get; set; }
    public required List<ManticoreHit> hits { get; set; }
}
