using Telegram.Bot.Types;

namespace HappyNotes.Services.interfaces;

public interface ITelegramService
{
    Task<Message> SendMessageAsync(string botToken, string channelId, string message, bool isMarkdown);
    Task<Message> SendLongMessageAsFileAsync(string botToken, string channelId, string message, string extension=".txt");
}
