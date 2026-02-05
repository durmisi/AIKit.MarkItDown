namespace AIKit.MarkItDown;

public class MarkItDownConversionException : Exception
{
    public MarkItDownConversionException(string message) : base(message)
    {
    }

    public MarkItDownConversionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}