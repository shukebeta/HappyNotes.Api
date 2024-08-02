namespace HappyNotes.Services.interfaces;

public interface ITelegramService
{
    Task SendMessageAsync(string botToken, string channelId, string message, bool isMarkdown);
    Task SendLongMessageAsFileAsync(string botToken, string channelId, string message, string extension=".txt");
}
