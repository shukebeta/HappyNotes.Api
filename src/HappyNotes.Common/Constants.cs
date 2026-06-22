namespace HappyNotes.Common;

public static class Constants
{
    public const int ShortNotesMaxLength = 1024;
    public const int PublicNotesMaxPage = 5;
    public const int MaxPageSize = 60;
    public const int TelegramMessageLength = 4096;
    public const int MastodonTootLength = 500;
    public const int FanfouStatusLength = 140;
    public const int TelegramCaptionLength = 1024;
    public const string TelegramSameTokenFlag = "the same token as the last setting";
    public const string MastodonAppName = "HappyNotes";
    public const string HappyNotesWebsite = "https://happynotes.shukebeta.com";

    // Sync service names
    public const string TelegramService = "telegram";
    public const string MastodonService = "mastodon";
    public const string ManticoreSearchService = "manticoresearch";
    public const string FanfouService = "fanfou";

    /// <summary>
    /// All available sync services
    /// </summary>
    public static readonly string[] AllSyncServices = { TelegramService, MastodonService, ManticoreSearchService, FanfouService };
}
