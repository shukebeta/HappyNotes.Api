namespace HappyNotes.Services.interfaces;

public interface IFanfouStatusService
{
    /// <summary>
    /// Send a short status (text only) to Fanfou
    /// </summary>
    Task<string> SendStatusAsync(
        string consumerKey,
        string consumerSecret,
        string accessToken,
        string accessTokenSecret,
        string content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a long status as photo with optional status text to Fanfou
    /// </summary>
    Task<string> SendStatusWithPhotoAsync(
        string consumerKey,
        string consumerSecret,
        string accessToken,
        string accessTokenSecret,
        byte[] photoData,
        string? statusText = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a status from Fanfou
    /// </summary>
    Task DeleteStatusAsync(
        string consumerKey,
        string consumerSecret,
        string accessToken,
        string accessTokenSecret,
        string statusId,
        CancellationToken cancellationToken = default);
}
