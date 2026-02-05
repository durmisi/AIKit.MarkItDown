namespace AIKit.MarkItDown;

public class MarkDownConfig
{
    public string? DocIntelEndpoint { get; set; }
    public string? DocIntelKey { get; set; }

    public string? OpenAiApiKey { get; set; }
    public string? LlmModel { get; set; }
    public string? LlmPrompt { get; set; }

    public bool KeepDataUris { get; set; }
    public bool EnablePlugins { get; set; }

    public List<string> Plugins { get; set; } = new List<string>();
}