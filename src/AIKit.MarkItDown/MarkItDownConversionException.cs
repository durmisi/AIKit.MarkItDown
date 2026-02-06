namespace AIKit.MarkItDown;

/// <summary>
/// Exception thrown when a Markdown conversion operation fails.
/// </summary>
public class MarkItDownConversionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarkItDownConversionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MarkItDownConversionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkItDownConversionException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public MarkItDownConversionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}