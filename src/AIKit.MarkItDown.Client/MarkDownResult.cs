namespace AIKit.MarkItDown.Client;

/// <summary>
/// Represents the result of a Markdown conversion.
/// </summary>
public class MarkDownResult
{
    /// <summary>
    /// The converted Markdown text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The title extracted from the document, if available.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Additional metadata from the conversion process.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}