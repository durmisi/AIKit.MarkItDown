using AIKit.MarkItDown;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIKit.MarkItDown.Client;

/// <summary>
/// Custom JSON converter for MarkDownConfig to serialize nested config objects as flattened properties.
/// </summary>
public class MarkDownConfigJsonConverter : JsonConverter<MarkDownConfig>
{
    public override MarkDownConfig Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var config = new MarkDownConfig();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "docintel_endpoint":
                        config.DocIntel ??= new DocIntelConfig();
                        config.DocIntel.Endpoint = reader.GetString();
                        break;
                    case "docintel_key":
                        config.DocIntel ??= new DocIntelConfig();
                        config.DocIntel.Key = reader.GetString();
                        break;
                    case "llm_api_key":
                        config.OpenAI ??= new OpenAIConfig();
                        config.OpenAI.ApiKey = reader.GetString();
                        break;
                    case "llm_model":
                        config.OpenAI ??= new OpenAIConfig();
                        config.OpenAI.Model = reader.GetString();
                        break;
                    case "llm_prompt":
                        config.LlmPrompt = reader.GetString();
                        break;
                    case "keep_data_uris":
                        config.KeepDataUris = reader.GetBoolean();
                        break;
                    case "enable_plugins":
                        config.EnablePlugins = reader.GetBoolean();
                        break;
                    case "plugins":
                        config.Plugins = JsonSerializer.Deserialize<List<string>>(ref reader, options);
                        break;
                }
            }
        }

        return config;
    }

    public override void Write(Utf8JsonWriter writer, MarkDownConfig value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.DocIntel?.Endpoint != null)
            writer.WriteString("docintel_endpoint", value.DocIntel.Endpoint);
        if (value.DocIntel?.Key != null)
            writer.WriteString("docintel_key", value.DocIntel.Key);
        if (value.OpenAI?.ApiKey != null)
            writer.WriteString("llm_api_key", value.OpenAI.ApiKey);
        if (value.OpenAI?.Model != null)
            writer.WriteString("llm_model", value.OpenAI.Model);
        if (value.LlmPrompt != null)
            writer.WriteString("llm_prompt", value.LlmPrompt);
        if (value.KeepDataUris.HasValue)
            writer.WriteBoolean("keep_data_uris", value.KeepDataUris.Value);
        if (value.EnablePlugins.HasValue)
            writer.WriteBoolean("enable_plugins", value.EnablePlugins.Value);
        if (value.Plugins != null)
        {
            writer.WritePropertyName("plugins");
            JsonSerializer.Serialize(writer, value.Plugins, options);
        }

        writer.WriteEndObject();
    }
}

/// <summary>
/// Configuration options for Markdown conversion in the client.
/// </summary>
[JsonConverter(typeof(MarkDownConfigJsonConverter))]
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
    /// Whether to enable plugins.
    /// </summary>
    public bool? EnablePlugins { get; set; }

    /// <summary>
    /// List of plugins to enable.
    /// </summary>
    public List<string>? Plugins { get; set; }
}