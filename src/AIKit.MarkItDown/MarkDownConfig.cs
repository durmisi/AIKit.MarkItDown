namespace AIKit.MarkItDown;

/// <summary>
/// Configuration for Azure Document Intelligence service.
/// </summary>
public class DocIntelConfig
{
    /// <summary>
    /// The endpoint URL for the Azure Document Intelligence service.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// The API key for authenticating with the Azure Document Intelligence service.
    /// </summary>
    public string? Key { get; set; }
}

/// <summary>
/// Configuration for OpenAI service.
/// </summary>
public class OpenAIConfig
{
    /// <summary>
    /// The API key for authenticating with OpenAI.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// The model name to use for OpenAI requests (e.g., "gpt-4").
    /// </summary>
    public string? Model { get; set; }
}

/// <summary>
/// Configuration options for Markdown conversion.
/// </summary>
public class MarkDownConfig
{
    /// <summary>
    /// Configuration for Azure Document Intelligence service.
    /// </summary>
    public DocIntelConfig? DocIntel { get; set; }

    /// <summary>
    /// Configuration for OpenAI service.
    /// </summary>
    public OpenAIConfig? OpenAI { get; set; }

    /// <summary>
    /// Custom prompt for the language model.
    /// </summary>
    public string? LlmPrompt { get; set; }

    /// <summary>
    /// Whether to keep data URIs in the output.
    /// </summary>
    public bool? KeepDataUris { get; set; }

    /// <summary>
    /// Whether to enable plugins during conversion.
    /// </summary>
    public bool? EnablePlugins { get; set; }

    /// <summary>
    /// List of plugin names to enable.
    /// </summary>
    public List<string> Plugins { get; set; } = new List<string>();
}