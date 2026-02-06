namespace AIKit.MarkItDown.Client;

/// <summary>
/// Request model for URI conversion.
/// </summary>
public class ConvertUriRequest
{
    /// <summary>
    /// The URI to convert.
    /// </summary>
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// Optional configuration for the conversion.
    /// </summary>
    public MarkDownConfig? Config { get; set; }
}