namespace HappyNotes.Models.Search;

public class ManticoreHit
{
    public long _id { get; set; }
    public long _score { get; set; }
    public required ManticoreSource _source { get; set; }
}
