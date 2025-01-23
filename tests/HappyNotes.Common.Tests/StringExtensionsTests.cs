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
    [TestCase(@"\#123 #124", "124")]
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

    [TestCase("@12345", "12345")]  // Valid case
    [TestCase("@1", "1")]  // Single digit
    [TestCase("@12345678901234567890123456789012", "12345678901234567890123456789012")]  // 32 digits
    [TestCase("@1234abc", "1234")]  // Valid followed by non-digit
    [TestCase("abc@1234", "1234")]  // Valid - with text before @
    [TestCase("@9876543210abc1234", "9876543210")]  // Valid - multiple patterns
    [TestCase("@01234", null)]  // Invalid - starts with 0
    [TestCase("@", null)]  // Invalid - no digits
    [TestCase("abcd", null)]  // Invalid - no @ symbol
    [TestCase("@123456789012345678901234567890123", null)]  // Invalid - more than 32 digits
    public void NoteIdRegex_ShouldMatchExpected(string input, string expected)
    {
        var matches = input.GetNoteIds();
        var result = matches.Any() ? matches[0][1..] : null;

        Assert.That(result, Is.EqualTo(expected));
    }
}
