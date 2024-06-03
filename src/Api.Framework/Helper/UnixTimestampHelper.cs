using System.Globalization;

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

    public static DateTimeOffset GetDateTimeOffset(string dateString, string dateFormat, string timeZoneId)
    {
        // Parse the date string to a DateTime object
        DateTime dateTime = DateTime.ParseExact(dateString, dateFormat, CultureInfo.InvariantCulture);

        // Get the specified time zone
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        // Convert the DateTime to the specified time zone
        DateTime dateTimeInTimeZone = TimeZoneInfo.ConvertTime(dateTime, timeZone);

        // Create a DateTimeOffset object with the correct offset
        DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTimeInTimeZone, timeZone.GetUtcOffset(dateTimeInTimeZone));

        return dateTimeOffset;
    }
}
