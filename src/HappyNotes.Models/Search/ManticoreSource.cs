namespace HappyNotes.Models.Search;

public class ManticoreSource
{
    public long Id { get; set; }
    public long userid { get; set; }
    public string content { get; set; } = string.Empty;
    public int islong { get; set; }
    public int isprivate { get; set; }
    public int ismarkdown { get; set; }
    public long createdat { get; set; }
    public long updatedat { get; set; }
    public long deletedat { get; set; }
}
