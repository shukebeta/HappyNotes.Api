namespace HappyNotes.Services.interfaces;

public interface ITelegramService
{
    Task<int> SendMessageAsync(string botToken, string channelId, string message, bool isMarkdown,
        CancellationToken cancellationToken = default);
    Task EditMessageAsync(string botToken, string chatId, int messageId, string newText, bool isMarkdown,
        CancellationToken cancellationToken = default);
    Task<int> SendLongMessageAsFileAsync(string botToken, string channelId, string message,
        string extension = ".txt", CancellationToken cancellationToken = default);
    Task DeleteMessageAsync(string botToken, string chatId, int messageId,
        CancellationToken cancellationToken = default);
}
