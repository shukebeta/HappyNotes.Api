using Telegram.Bot.Types;

namespace HappyNotes.Services.interfaces;

public interface ITelegramService
{
    Task<Message> SendMessageAsync(string botToken, string channelId, string message, bool isMarkdown);
    Task<Message> EditMessageAsync(string botToken, string chatId, int messageId, string newText, bool isMarkdown);
    Task<Message> SendLongMessageAsFileAsync(string botToken, string channelId, string message, string extension = ".txt");
    Task DeleteMessageAsync(string botToken, string chatId, int messageId);
}
