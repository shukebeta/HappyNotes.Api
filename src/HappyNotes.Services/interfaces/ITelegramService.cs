using System.Threading;
using Telegram.Bot.Types;

namespace HappyNotes.Services.interfaces;

public interface ITelegramService
{
    Task<Message> SendMessageAsync(string botToken, string channelId, string message, bool isMarkdown,
        CancellationToken cancellationToken = default);
    Task<Message> EditMessageAsync(string botToken, string chatId, int messageId, string newText, bool isMarkdown,
        CancellationToken cancellationToken = default);
    Task<Message> SendLongMessageAsFileAsync(string botToken, string channelId, string message,
        string extension = ".txt", CancellationToken cancellationToken = default);
    Task DeleteMessageAsync(string botToken, string chatId, int messageId,
        CancellationToken cancellationToken = default);
}
