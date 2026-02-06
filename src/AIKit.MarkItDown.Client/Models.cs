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

/// <summary>
/// Configuration options for Markdown conversion in the client.
/// </summary>
public class MarkDownConfig
{
    /// <summary>
    /// The endpoint for Azure Document Intelligence.
    /// </summary>
    public string? DocIntelEndpoint { get; set; }

    /// <summary>
    /// The API key for the language model service.
    /// </summary>
    public string? LlmApiKey { get; set; }

    /// <summary>
    /// The model name for the language model.
    /// </summary>
    public string? LlmModel { get; set; }

    /// <summary>
    /// Custom prompt for the language model.
    /// </summary>
    public string? LlmPrompt { get; set; }

    /// <summary>
    /// Whether to keep data URIs in the output.
    /// </summary>
    public bool? KeepDataUris { get; set; }

    /// <summary>
    /// Whether to enable plugins.
    /// </summary>
    public bool? EnablePlugins { get; set; }

    /// <summary>
    /// The key for Azure Document Intelligence.
    /// </summary>
    public string? DocIntelKey { get; set; }

    /// <summary>
    /// List of plugins to enable.
    /// </summary>
    public List<string>? Plugins { get; set; }
}

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