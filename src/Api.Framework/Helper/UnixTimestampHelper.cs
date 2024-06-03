namespace Api.Framework.Helper;

public static class UnixTimestampHelper
{
    public static (long StartUnixTimestamp, long EndUnixTimestamp) GetDayUnixTimestamps(string localTimezone,
        DateTimeOffset? aDate = null)
    {
        DateTimeOffset date = aDate ?? DateTimeOffset.UtcNow;

        // Create a DateTimeOffset instance for the specified date and time zone
        DateTimeOffset localDateTime = new DateTimeOffset(date.DateTime,
            TimeZoneInfo.FindSystemTimeZoneById(localTimezone).GetUtcOffset(date.DateTime));

        // Get the start of the day in the local time zone
        DateTimeOffset startOfDay = new DateTimeOffset(localDateTime.Year, localDateTime.Month, localDateTime.Day, 0, 0,
            0, localDateTime.Offset);

        // Get the end of the day in the local time zone
        DateTimeOffset endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        // Convert the start and end of the day to Unix timestamps
        long startUnixTimestamp = startOfDay.ToUnixTimeSeconds();
        long endUnixTimestamp = endOfDay.ToUnixTimeSeconds();

        return (startUnixTimestamp, endUnixTimestamp);
    }
}
