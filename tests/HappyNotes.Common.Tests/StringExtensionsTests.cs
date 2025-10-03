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
        var input = shortStr + new string('y', 1);
        // Act & Assert
        Assert.That(input.GetShort(), Is.EqualTo(shortStr));
    }

    [TestCase("#123", "123")]
    [TestCase("#123#124", "123")]
    [TestCase("#123 #124", "123")]
    [TestCase(@"\#123 #124", "124")]
    [TestCase(@"`\#123 #124\``#333", "333")]
    [TestCase(@"`\#123 \` #124`#333", "333")]
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
        var result = string.Join(" ", input.GetTags());

        // Assert
        Assert.That(result, Is.EqualTo("123中 234国 345人 456民 567还没站起来"));
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
    [TestCase(@"\@1234567890", null)]  // Invalid - @ is escaped
    public void NoteIdRegex_ShouldMatchExpected(string input, string expected)
    {
        var matches = input.GetNoteIds();
        var result = matches.Any() ? matches[0][1..] : null;

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("this is a note image 1 image 2 image 3", "this is a note")]
    [TestCase("this is a note image 1 image 2", "this is a note")]
    [TestCase("this is a note image 1", "this is a note")]
    [TestCase("this is a note", "this is a note")]
    [TestCase(null, "")]
    [TestCase("this is a note see image 1: bird", "this is a note see image 1: bird")]
    [TestCase("this is a note see image 1: bird image 2", "this is a note see image 1: bird")]
    public void RemoveImageReferenceTest(string input, string expected)
    {
        // Act
        var result = input.RemoveImageReference();

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("<p>hello, world</p>", true)]
    [TestCase("<p/>", true)]
    [TestCase("<br/>", true)]
    [TestCase("<https://abc.com/>", false)]
    [TestCase("<http://abc.com/>", false)]
    public void IsHtmlTest(string input, bool expected)
    {
        // Act
        var result = input.IsHtml();

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("@123 This is content", "This is content")]
    [TestCase("@123 @456 This is content", "This is content")]
    [TestCase("This is content @123", "This is content")]
    [TestCase("This is content @123 @456", "This is content")]
    [TestCase("@123 This is content @456", "This is content")]
    [TestCase("@123 @456 This is content @789", "This is content")]
    [TestCase("This is content", "This is content")]
    [TestCase("@123", "")]
    [TestCase("@123 @456 @789", "")]
    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("This @123 is content", "This @123 is content")]
    [TestCase(@"\@123 This is content", @"\@123 This is content")]
    [TestCase("@123\nThis is content", "This is content")]
    [TestCase("This is content\n@123", "This is content")]
    public void RemoveNoteLinksTest(string input, string expected)
    {
        // Act
        var result = input.RemoveNoteLinks();

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }


    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("Hello\nWorld", "Hello\nWorld")]  // Unix LF - unchanged
    [TestCase("Hello\r\nWorld", "Hello\nWorld")]  // Windows CRLF → LF
    [TestCase("Hello\rWorld", "Hello\nWorld")]  // Old Mac CR → LF
    [TestCase("Hello\u2028World", "Hello\nWorld")]  // Unicode Line Separator → LF
    [TestCase("Hello\u2029World", "Hello\n\nWorld")]  // Unicode Paragraph Separator → LF LF
    [TestCase("Line1\r\nLine2\rLine3\nLine4", "Line1\nLine2\nLine3\nLine4")]  // Mixed formats
    [TestCase("A\r\n\r\nB", "A\n\nB")]  // Multiple Windows line breaks
    [TestCase("A\r\rB", "A\n\nB")]  // Multiple old Mac line breaks
    [TestCase("Mixed\r\n\rformats\u2028here", "Mixed\n\nformats\nhere")]  // All types mixed
    public void NormalizeNewlinesTest(string input, string expected)
    {
        // Act
        var result = input.NormalizeNewlines();

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }
}
