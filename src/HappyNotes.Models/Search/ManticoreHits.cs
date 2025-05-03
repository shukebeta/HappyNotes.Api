namespace HappyNotes.Models.Search;

public class ManticoreHits
{
    public long total { get; set; }
    public List<ManticoreHit> hits { get; set; }
}