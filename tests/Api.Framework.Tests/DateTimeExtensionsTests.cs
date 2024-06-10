using Api.Framework.Extensions;

namespace Api.Framework.Tests;

public class DateTimeExtensionsTests
{
    [Test]
    public void ToUnixTimestampTest()
    {
        var time = DateTime.UnixEpoch;
        Assert.That(time.ToUnixTimeSeconds(), Is.EqualTo(0));
    }

    [TestCase(1717665363, "2024-06-06 21:16:03")]
    public void ToDateTimeOffset_ToYmdHms_Test(long timestamp, string expected)
    {
        var timeZoneId = "Pacific/Auckland";
        var dateTimeOffset = timestamp.ToDateTimeOffset(timeZoneId);
        Assert.That(dateTimeOffset.ToYmdHms(), Is.EqualTo(expected));

        // one year ago
        dateTimeOffset = dateTimeOffset.AddYears(-1);
        Assert.That(dateTimeOffset.ToYmdHms(), Is.EqualTo(expected.Replace("2024", "2023")));
    }
}
