using Api.Framework.Exceptions;
using Api.Framework.Helper;

namespace Api.Framework.Tests;

[TestFixture]
public class CustomExceptionTests
{
    [Test]
    public void CustomException_WithMessage_SetsCorrectExceptionMessage()
    {
        // Arrange
        const string expectedMessage = "Test error message";

        // Act
        var exception = new CustomException<object>(expectedMessage);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void CustomException_WithNullMessage_UsesDefaultMessage()
    {
        // Arrange & Act
        var exception = new CustomException<object>();

        // Assert
        Assert.That(exception.Message, Is.EqualTo("A custom exception happened"));
    }

    [Test]
    public void CustomException_WithInnerExceptionButNoMessage_UsesInnerExceptionMessage()
    {
        // Arrange
        const string innerMessage = "Inner exception message";
        var innerException = new InvalidOperationException(innerMessage);

        // Act
        var exception = new CustomException<object>(null, innerException);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(innerMessage));
        Assert.That(exception.InnerException, Is.EqualTo(innerException));
    }

    [Test]
    public void CustomExceptionHelper_New_CreatesExceptionWithCorrectMessage()
    {
        // Arrange
        const string errorMessage = "Note with id {0} not found";
        const long noteId = 123L;
        const string expectedMessage = "Note with id 123 not found";

        // Act
        var exception = CustomExceptionHelper.New("test data", errorMessage, noteId);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
        Assert.That(exception.CustomData?.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void CustomExceptionHelper_NewWithErrorCode_CreatesExceptionWithCorrectMessage()
    {
        // Arrange
        const string errorMessage = "User {0} has {1} notes";
        const int errorCode = 100;
        const string userName = "testuser";
        const int noteCount = 5;
        const string expectedMessage = "User testuser has 5 notes";

        // Act
        var exception = CustomExceptionHelper.New("test data", errorCode, errorMessage, userName, noteCount);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
        Assert.That(exception.CustomData?.Message, Is.EqualTo(expectedMessage));
        Assert.That(exception.CustomData?.ErrorCode, Is.EqualTo(errorCode));
    }
}