namespace AIKit.MarkItDown.Client;

public class MarkDownResult
{
    public string Text { get; set; } = string.Empty;
    public string? Title { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class MarkDownConfig
{
    public string? DocIntelEndpoint { get; set; }
    public string? LlmApiKey { get; set; }
    public string? LlmModel { get; set; }
    public string? LlmPrompt { get; set; }
    public bool? KeepDataUris { get; set; }
    public bool? EnablePlugins { get; set; }
    public string? DocIntelKey { get; set; }
    public List<string>? Plugins { get; set; }
}

public class ConvertUriRequest
{
    public string Uri { get; set; } = string.Empty;
    public MarkDownConfig? Config { get; set; }
}