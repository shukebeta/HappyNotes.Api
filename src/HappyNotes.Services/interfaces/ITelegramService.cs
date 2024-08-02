namespace HappyNotes.Services.interfaces;

public interface ITelegramService
{
    Task SendMessageAsync(string botToken, string channelId, string message);
    Task SendFileAsync(string botToken, string channelId, string filePath, string caption = null);
    Task SendLongMessageAsFileAsync(string botToken, string channelId, string message, string tempFilePath = "note.txt");
}
