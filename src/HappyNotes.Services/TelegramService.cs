using HappyNotes.Common;
using HappyNotes.Services.interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

namespace HappyNotes.Services;

public class TelegramService : ITelegramService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TelegramService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<int> SendMessageAsync(string botToken, string channelId, string message, bool isMarkdown,
        CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient("TelegramBotClient");
        var botClient = new TelegramBotClient(botToken, httpClient);
        var result = await botClient.SendMessage(
            chatId: _GetChatId(channelId),
            text: message,
            parseMode: isMarkdown ? ParseMode.Markdown : ParseMode.None,
            cancellationToken: cancellationToken
        );
        return result.MessageId;
    }

    public async Task<int> SendLongMessageAsFileAsync(string botToken, string channelId, string message,
        string extension = ".txt", CancellationToken cancellationToken = default)
    {
        string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + extension);

        try
        {
            await File.WriteAllTextAsync(tempFilePath, message, cancellationToken);

            var httpClient = _httpClientFactory.CreateClient("TelegramBotClient");
            return await _SendFileAsync(botToken, channelId, tempFilePath, message, httpClient, cancellationToken);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    public async Task EditMessageAsync(string botToken, string chatId, int messageId, string newText,
        bool isMarkdown, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient("TelegramBotClient");
        var botClient = new TelegramBotClient(botToken, httpClient);

        await botClient.EditMessageText(
            chatId: _GetChatId(chatId),
            messageId: messageId,
            text: newText,
            parseMode: isMarkdown ? ParseMode.Markdown : ParseMode.None,
            cancellationToken: cancellationToken
        );
    }

    public async Task DeleteMessageAsync(string botToken, string chatId, int messageId,
        CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient("TelegramBotClient");
        var botClient = new TelegramBotClient(botToken, httpClient);

        await botClient.DeleteMessage(
            chatId: _GetChatId(chatId),
            messageId: messageId,
            cancellationToken: cancellationToken
        );
    }

    private async Task<int> _SendFileAsync(string botToken, string channelId, string filePath,
        string message, HttpClient httpClient, CancellationToken cancellationToken)
    {
        var botClient = new TelegramBotClient(botToken, httpClient);
        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var inputFile = InputFile.FromStream(fileStream, Path.GetFileName(filePath));
        var isMarkdown = filePath.EndsWith(".md");
        var result = await botClient.SendDocument(
            chatId: _GetChatId(channelId),
            document: inputFile,
            caption: _GetTelegramCaption(message),
            parseMode: isMarkdown ? ParseMode.Markdown : ParseMode.None,
            cancellationToken: cancellationToken
        );
        return result.MessageId;
    }

    private static string _GetTelegramCaption(string? message)
    {
        var caption = message?.GetShort(Constants.TelegramCaptionLength - 3) ?? "";
        if (caption.Length == Constants.TelegramCaptionLength - 3)
        {
            return caption + "...";
        }

        return caption;
    }

    private static ChatId _GetChatId(string chatId)
    {
        if (chatId.StartsWith("@"))
        {
            return new ChatId(chatId);
        }

        return long.Parse(chatId);
    }
}
