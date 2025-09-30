namespace Api.Framework.Extensions;

public static class TimeProviderExtensions
{
    /// <summary>
    /// Gets the current UTC time as Unix timestamp in seconds.
    /// </summary>
    /// <param name="timeProvider">The time provider</param>
    /// <returns>Unix timestamp in seconds</returns>
    public static long GetUtcNowUnixTimeSeconds(this TimeProvider timeProvider)
    {
        return timeProvider.GetUtcNow().ToUnixTimeSeconds();
    }

    /// <summary>
    /// Gets the current local time as Unix timestamp in seconds.
    /// </summary>
    /// <param name="timeProvider">The time provider</param>
    /// <returns>Unix timestamp in seconds</returns>
    public static long GetLocalNowUnixTimeSeconds(this TimeProvider timeProvider)
    {
        return timeProvider.GetLocalNow().ToUnixTimeSeconds();
    }
}