using HappyNotes.Common;

namespace HappyNotes.Extensions.Tests;

public class StringExtensionsTests
{
    private string LongString => new string('x', Constants.ShortNotesMaxLength + 1);
    private string ShortString => new string('x', Constants.ShortNotesMaxLength - 1);
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void IsLongTest_ShortIsShort()
    {
        Assert.IsFalse(ShortString.IsLong());
    }

    [Test]
    public void IsLongTest_AutoLong()
    {
        Assert.IsTrue(LongString.IsLong());
    }

    [TestCase(null, false)]
    [TestCase("A\r\r\r\rB", true)]
    [TestCase("A\n\n\n\nB", true)]
    [TestCase("A\r\n\r\n\r\n\r\nB", true)]
    [TestCase("A<!-- more -->B", true)]
    public void IsLongTest_ManualLong(string input, bool expected)
    {
        Assert.That(input.IsLong(), Is.EqualTo(expected));
    }

    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("AB", "AB")]
    [TestCase("A\r\r\r\rB", "A")]
    [TestCase("A\n\n\n\nB", "A")]
    [TestCase("A\r\n\r\n\r\n\r\nB", "A")]
    [TestCase("A<!-- more -->B", "A")]
    public void GetShort_Short(string input, string expected)
    {
        Assert.That(input.GetShort(), Is.EqualTo(expected));
    }

    [Test]
    public void GetShort_Long()
    {
        // Arrange
        var shortStr = new string('x', Constants.ShortNotesMaxLength);
        var input =  shortStr + new string('y', 1);
        // Act & Assert
        Assert.That(input.GetShort(), Is.EqualTo(shortStr));
    }

    [TestCase("#123", "123")]
    [TestCase("#123#124", "123")]
    [TestCase("#123 #124", "123")]
    public void GetTags_SingleLine(string input, string expected)
    {
        // Act
        var tag = input.GetTags().FirstOrDefault();

        // Assert
        Assert.That(tag, Is.EqualTo(expected));
    }

    [Test]
    public void GetTags_Multiline()
    {
        var input = @"#123中
#234国 
#345人
#456民#567还没站起来";
        // Act
        var tag = string.Join(" ", input.GetTags());

        // Assert
        Assert.That(tag, Is.EqualTo("123中 234国 345人 456民 567还没站起来"));
    }
}
