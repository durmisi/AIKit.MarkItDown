namespace AIKit.MarkItDown;

public class DocIntelConfig
{
    public string? Endpoint { get; set; }
    public string? Key { get; set; }
}

public class OpenAIConfig
{
    public string? ApiKey { get; set; }
    public string? Model { get; set; }
}

public class MarkDownConfig
{
    public DocIntelConfig? DocIntel { get; set; }
    public OpenAIConfig? OpenAI { get; set; }

    public string? LlmPrompt { get; set; }

    public bool? KeepDataUris { get; set; }
    public bool? EnablePlugins { get; set; }

    public List<string> Plugins { get; set; } = new List<string>();
}