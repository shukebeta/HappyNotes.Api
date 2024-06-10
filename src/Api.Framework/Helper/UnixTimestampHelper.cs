using System.Globalization;
using Api.Framework.Extensions;

namespace Api.Framework.Helper;

public static class UnixTimestampHelper
{
    public static (long StartUnixTimestamp, long EndUnixTimestamp) GetDayUnixTimestamps(string localTimezone,
        DateTimeOffset? aDate = null)
    {
        DateTimeOffset date = aDate ?? DateTimeOffset.UtcNow;
        long startUnixTimestamp = date.GetDayStartTimestamp(TimeZoneInfo.FindSystemTimeZoneById(localTimezone));
        return (startUnixTimestamp, startUnixTimestamp + 86400 - 1);
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
        DateTimeOffset dateTimeOffset =
            new DateTimeOffset(dateTimeInTimeZone, timeZone.GetUtcOffset(dateTimeInTimeZone));

        return dateTimeOffset;
    }
}
