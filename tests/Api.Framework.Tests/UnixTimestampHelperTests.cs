using Api.Framework.Helper;
namespace Api.Framework.Tests;

[TestFixture]
public class UnixTimestampHelperTests
{
    [TestCase("Pacific/Auckland")]
    [TestCase("New Zealand Standard Time")]
    [TestCase("UTC+12")]
    public void GetDayUnixTimestamps_WithDateTimeOffsetAndTimezone_ReturnsCorrectTimestamps(string localTimezone)
    {
        // Arrange
        DateTimeOffset date = new DateTimeOffset(2024, 6, 3, 12, 0, 0, TimeSpan.FromHours(12));
        DateTimeOffset dateStart = new DateTimeOffset(2024, 6, 3, 0, 0, 0, TimeSpan.FromHours(12));

        // Act
        (long startUnixTimestamp, long endUnixTimestamp) =
            UnixTimestampHelper.GetDayUnixTimestamps(localTimezone, date);

        // Assert
        Assert.That(startUnixTimestamp, Is.EqualTo(dateStart.ToUnixTimeSeconds()));
        Assert.That(endUnixTimestamp, Is.EqualTo(dateStart.ToUnixTimeSeconds() + 86400 - 1));
    }

    [Test]
    public void GetDayUnixTimestamps_WithNullDateTimeOffset_ReturnsCurrentDayTimestamps()
    {
        // Arrange
        string localTimezone = "America/Los_Angeles";
        DateTimeOffset currentUtcTime = DateTimeOffset.UtcNow;

        // Act
        (long startUnixTimestamp, long endUnixTimestamp) =
            UnixTimestampHelper.GetDayUnixTimestamps(localTimezone, null);

        // Assert
        DateTimeOffset startOfDay = new DateTimeOffset(currentUtcTime.Year, currentUtcTime.Month, currentUtcTime.Day, 0,
            0, 0, TimeZoneInfo.FindSystemTimeZoneById(localTimezone).GetUtcOffset(currentUtcTime.DateTime));
        DateTimeOffset endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        long expectedStartUnixTimestamp = startOfDay.ToUnixTimeSeconds();
        long expectedEndUnixTimestamp = endOfDay.ToUnixTimeSeconds();

        Assert.That(startUnixTimestamp, Is.EqualTo(expectedStartUnixTimestamp));
        Assert.That(endUnixTimestamp, Is.EqualTo(expectedEndUnixTimestamp));
    }
}
