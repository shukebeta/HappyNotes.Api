using HappyNotes.Services.interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
namespace HappyNotes.Services;

public class TelegramService : ITelegramService
{
    public async Task SendMessageAsync(string botToken, string channelId, string message)
    {
        var botClient = new TelegramBotClient(botToken);
        await botClient.SendTextMessageAsync(
            chatId: channelId,
            text: message
        );
    }

    public async Task SendFileAsync(string botToken, string channelId, string filePath, string caption = null)
    {
        var botClient = new TelegramBotClient(botToken);
        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var inputOnlineFile = new InputOnlineFile(fileStream, Path.GetFileName(filePath));

        await botClient.SendDocumentAsync(
            chatId: channelId,
            document: inputOnlineFile,
            caption: caption
        );
    }

    public async Task SendLongMessageAsFileAsync(string botToken, string channelId, string message, string tempFilePath = "note.txt")
    {
        await File.WriteAllTextAsync(tempFilePath, message);
        await SendFileAsync(botToken, channelId, tempFilePath, "Here is the long note as a text file.");
    }
}
