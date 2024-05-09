using Api.Framework.Extensions;

namespace Api.Framework.Tests;

public class DateTimeExtensionsTests
{
    [Test]
    public void ToUnixTimestampTest()
    {
        var time = DateTime.UnixEpoch;
        Assert.That(time.ToUnixTimestamp(), Is.EqualTo(0));
    }
}
