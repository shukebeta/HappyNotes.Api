using Api.Framework.Exceptions;
using Api.Framework.Helper;

namespace Api.Framework.Tests;

[TestFixture]
public class CustomExceptionTests
{
    [Test]
    public void CustomException_ViaHelper_SetsCustomDataMessage()
    {
        // Arrange
        const string expectedMessage = "Test error message";

        // Act
        var exception = CustomExceptionHelper.New<object>(new object(), expectedMessage);

        // Assert
        Assert.That(exception.CustomData?.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void CustomException_WrapsInnerException_ExposesInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner exception message");

        // Act
        var exception = new CustomException<object>(innerException);

        // Assert
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
        Assert.That(exception.CustomData?.Message, Is.EqualTo(expectedMessage));
        Assert.That(exception.CustomData?.ErrorCode, Is.EqualTo(errorCode));
    }
}
