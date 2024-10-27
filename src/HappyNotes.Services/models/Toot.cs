namespace HappyNotes.Services.models;

public class Toot
{
    public string Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RepliesCount { get; set; }
    public int ReblogsCount { get; set; }
    public int FavouritesCount { get; set; }
}
