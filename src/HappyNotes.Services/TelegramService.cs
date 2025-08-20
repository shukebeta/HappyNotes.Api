using System.Net.Http;
using HappyNotes.Common;
using HappyNotes.Services.interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace HappyNotes.Services;

public class TelegramService : ITelegramService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TelegramService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Message> SendMessageAsync(string botToken, string channelId, string message, bool isMarkdown)
    {
        var httpClient = _httpClientFactory.CreateClient("TelegramBotClient");
        var botClient = new TelegramBotClient(botToken, httpClient);
        return await botClient.SendTextMessageAsync(
            chatId: _GetChatId(channelId),
            text: message,
            isMarkdown ? ParseMode.Markdown : null
        );
    }

    public async Task<Message> SendLongMessageAsFileAsync(string botToken, string channelId, string message,
        string extension = ".txt")
    {
        // Create a temporary file in the system's temporary folder
        string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + extension);

        try
        {
            // Write the message to the temporary file
            await File.WriteAllTextAsync(tempFilePath, message);

            // Send the file via Telegram
            var httpClient = _httpClientFactory.CreateClient("TelegramBotClient");
            return await _SendFileAsync(botToken, channelId, tempFilePath, message, httpClient);
        }
        finally
        {
            // Ensure the temporary file is deleted after use
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    public async Task<Message> EditMessageAsync(string botToken, string chatId, int messageId, string newText,
        bool isMarkdown)
    {
        var httpClient = _httpClientFactory.CreateClient("TelegramBotClient");
        var botClient = new TelegramBotClient(botToken, httpClient);

        return await botClient.EditMessageTextAsync(
            chatId: _GetChatId(chatId),
            messageId: messageId,
            text: newText,
            parseMode: isMarkdown ? ParseMode.Markdown : null
        );
    }

    public async Task DeleteMessageAsync(string botToken, string chatId, int messageId)
    {
        var httpClient = _httpClientFactory.CreateClient("TelegramBotClient");
        var botClient = new TelegramBotClient(botToken, httpClient);

        await botClient.DeleteMessageAsync(
            chatId: _GetChatId(chatId),
            messageId: messageId
        );
    }


    private async Task<Message> _SendFileAsync(string botToken, string channelId, string filePath,
        string message, HttpClient httpClient)
    {
        var botClient = new TelegramBotClient(botToken, httpClient);
        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var inputOnlineFile = new InputOnlineFile(fileStream, Path.GetFileName(filePath));
        var isMarkdown = filePath.EndsWith(".md");
        return await botClient.SendDocumentAsync(
            chatId: _GetChatId(channelId),
            document: inputOnlineFile,
            caption: _GetTelegramCaption(message),
            parseMode: isMarkdown ? ParseMode.Markdown : null
        );
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
