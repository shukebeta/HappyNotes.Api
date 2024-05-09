namespace Api.Framework.Extensions;

public static class DateTimeExtensions
{
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        TimeSpan timeSpan = dateTime.ToUniversalTime() - DateTime.UnixEpoch;
        return (long) timeSpan.TotalSeconds;
    }
}
