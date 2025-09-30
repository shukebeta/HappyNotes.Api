using Api.Framework.Result;

namespace Api.Framework.Exceptions;

public class CustomException<T> : Exception
{
    public CustomException(string? message = null, Exception? innerException = null)
        : base(message ?? innerException?.Message ?? "A custom exception happened", innerException)
    {
    }

    public FailedResult<T>? CustomData { get; set; }

    // Override ToString() method to provide more detailed exception information
    public override string ToString()
    {
        return $"{base.ToString()}, {CustomData}";
    }
}
