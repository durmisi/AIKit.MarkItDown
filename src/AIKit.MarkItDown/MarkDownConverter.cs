using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIKit.MarkItDown;

/// <summary>
/// Provides functionality to convert various file formats to Markdown using the markitdown library.
/// This class handles file, stream, and URI conversions with optional configuration for AI services.
/// </summary>
public class MarkDownConverter
{
    /// <summary>
    /// Maximum allowed stream size in bytes (100 MB).
    /// </summary>
    private const long MaxStreamSizeBytes = 100 * 1024 * 1024; // 100 MB

    /// <summary>
    /// Default timeout in milliseconds for conversion operations (30 seconds).
    /// </summary>
    private const int DefaultTimeoutMs = 30_000;

    /// <summary>
    /// Path to the worker executable used for Python integration.
    /// </summary>
    private static readonly string WorkerExePath;

    /// <summary>
    /// Default configuration for conversions.
    /// </summary>
    private readonly MarkDownConfig _defaultConfig;

    /// <summary>
    /// Static constructor that initializes the worker executable path.
    /// Ensures the worker exe exists in the assembly directory.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the worker executable is not found.</exception>
    static MarkDownConverter()
    {
        var assemblyDir = Path.GetDirectoryName(typeof(MarkDownConverter).Assembly.Location)!;
        WorkerExePath = Path.Combine(assemblyDir, "AIKit.MarkItDown.Worker.exe");
        if (!File.Exists(WorkerExePath))
            throw new InvalidOperationException("Worker exe not found.");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkDownConverter"/> class with default configuration.
    /// </summary>
    public MarkDownConverter()
    {
        _defaultConfig = new MarkDownConfig();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkDownConverter"/> class with custom configuration.
    /// </summary>
    /// <param name="docIntelConfig">Configuration for Azure Document Intelligence service. Can be null.</param>
    /// <param name="openAiConfig">Configuration for OpenAI service. Can be null.</param>
    /// <exception cref="ArgumentException">Thrown if required fields in configs are null or empty.</exception>
    public MarkDownConverter(
        DocIntelConfig? docIntelConfig = null,
        OpenAIConfig? openAiConfig = null)
    {
        if (docIntelConfig != null)
        {
            if (string.IsNullOrEmpty(docIntelConfig.Endpoint))
                throw new ArgumentException("DocIntelConfig.Endpoint cannot be null or empty.", nameof(docIntelConfig));
            if (string.IsNullOrEmpty(docIntelConfig.Key))
                throw new ArgumentException("DocIntelConfig.Key cannot be null or empty.", nameof(docIntelConfig));
        }

        if (openAiConfig != null)
        {
            if (string.IsNullOrEmpty(openAiConfig.ApiKey))
                throw new ArgumentException("OpenAIConfig.ApiKey cannot be null or empty.", nameof(openAiConfig));
            if (string.IsNullOrEmpty(openAiConfig.Model))
                throw new ArgumentException("OpenAIConfig.Model cannot be null or empty.", nameof(openAiConfig));
        }

        _defaultConfig = new MarkDownConfig
        {
            DocIntel = docIntelConfig,
            OpenAI = openAiConfig
        };
    }

    /* ---------------------------------------------------------
     * PUBLIC ASYNC API
     * --------------------------------------------------------- */

    /// <summary>
    /// Converts a file to Markdown asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the file to convert.</param>
    /// <param name="config">Optional configuration for the conversion. If null, default config is used.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the Markdown string.</returns>
    /// <exception cref="MarkItDownConversionException">Thrown if conversion fails.</exception>
    public Task<string> ConvertAsync(
        string filePath,
        MarkDownConfig? config = null,
        CancellationToken ct = default)
        => RunAsync((ct) => ConvertInternalAsync(filePath, config, ct), ct);

    /// <summary>
    /// Converts a stream to Markdown asynchronously.
    /// </summary>
    /// <param name="stream">The stream containing the data to convert.</param>
    /// <param name="extension">The file extension indicating the format (e.g., ".pdf").</param>
    /// <param name="config">Optional configuration for the conversion. If null, default config is used.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the Markdown string.</returns>
    /// <exception cref="MarkItDownConversionException">Thrown if conversion fails or stream is too large.</exception>
    public Task<string> ConvertAsync(
        Stream stream,
        string extension,
        MarkDownConfig? config = null,
        CancellationToken ct = default)
        => RunAsync((ct) => ConvertInternalAsync(stream, extension, config, ct), ct);

    /// <summary>
    /// Converts a URI to Markdown asynchronously.
    /// </summary>
    /// <param name="uri">The URI of the resource to convert.</param>
    /// <param name="config">Optional configuration for the conversion. If null, default config is used.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the Markdown string.</returns>
    /// <exception cref="MarkItDownConversionException">Thrown if conversion fails.</exception>
    public Task<string> ConvertUriAsync(
        string uri,
        MarkDownConfig? config = null,
        CancellationToken ct = default)
        => RunAsync((ct) => ConvertUriInternalAsync(uri, config, ct), ct);

    /* ---------------------------------------------------------
     * PUBLIC SYNC API
     * --------------------------------------------------------- */

    /// <summary>
    /// Converts a file to Markdown synchronously.
    /// </summary>
    /// <param name="filePath">The path to the file to convert.</param>
    /// <param name="config">Optional configuration for the conversion. If null, default config is used.</param>
    /// <returns>The Markdown string.</returns>
    /// <exception cref="MarkItDownConversionException">Thrown if conversion fails.</exception>
    public string Convert(
        string filePath,
        MarkDownConfig? config = null)
        => ConvertInternalAsync(filePath, config, default).Result;

    /// <summary>
    /// Converts a stream to Markdown synchronously.
    /// </summary>
    /// <param name="stream">The stream containing the data to convert.</param>
    /// <param name="extension">The file extension indicating the format (e.g., ".pdf").</param>
    /// <param name="config">Optional configuration for the conversion. If null, default config is used.</param>
    /// <returns>The Markdown string.</returns>
    /// <exception cref="MarkItDownConversionException">Thrown if conversion fails or stream is too large.</exception>
    public string Convert(
        Stream stream,
        string extension,
        MarkDownConfig? config = null)
        => ConvertInternalAsync(stream, extension, config, default).Result;

    /// <summary>
    /// Converts a URI to Markdown synchronously.
    /// </summary>
    /// <param name="uri">The URI of the resource to convert.</param>
    /// <param name="config">Optional configuration for the conversion. If null, default config is used.</param>
    /// <returns>The Markdown string.</returns>
    /// <exception cref="MarkItDownConversionException">Thrown if conversion fails.</exception>
    public string ConvertUri(
        string uri,
        MarkDownConfig? config = null)
        => ConvertUriInternalAsync(uri, config, default).Result;

    /* ---------------------------------------------------------
     * ASYNC EXECUTION WRAPPER
     * --------------------------------------------------------- */

    /// <summary>
    /// Executes a work function with a timeout.
    /// </summary>
    /// <param name="work">The work function to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="timeoutMs">Timeout in milliseconds.</param>
    /// <returns>The result of the work function.</returns>
    private static async Task<string> RunAsync(
        Func<CancellationToken, Task<string>> work,
        CancellationToken ct,
        int timeoutMs = DefaultTimeoutMs)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeoutMs);

        return await work(cts.Token);
    }

    /// <summary>
    /// Runs the worker process to perform the conversion using Python.
    /// Serializes input to JSON, sends to worker, and deserializes result.
    /// </summary>
    /// <param name="input">The input object to pass to the worker.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The conversion result as a string.</returns>
    /// <exception cref="MarkItDownConversionException">Thrown if the worker process fails or returns an error.</exception>
    private static async Task<string> RunWorkerAsync(object input, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(input);
        var psi = new ProcessStartInfo
        {
            FileName = WorkerExePath,
            Arguments = "",
            WorkingDirectory = Path.GetDirectoryName(WorkerExePath),
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        using var writer = process.StandardInput;
        await writer.WriteAsync(json);
        writer.Close();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await Task.WhenAll(outputTask, process.WaitForExitAsync(ct));

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
            throw new MarkItDownConversionException($"Worker process failed: {error}");

        Console.WriteLine($"Worker output: '{output.Trim()}'");
        var result = JsonSerializer.Deserialize<WorkerResult>(output);
        if (result == null || !result.Success)
            throw new MarkItDownConversionException(result?.Error ?? "Unknown error");

        return result.Result;
    }

    /// <summary>
    /// Represents the result from the worker process.
    /// </summary>
    /// <param name="Success">Indicates if the operation was successful.</param>
    /// <param name="Result">The resulting Markdown string if successful.</param>
    /// <param name="Error">The error message if not successful.</param>
    private record WorkerResult(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("result")] string? Result,
        [property: JsonPropertyName("error")] string? Error);

    /* ---------------------------------------------------------
     * SYNC PYTHON CORE (GIL OWNERSHIP) - Wait, this comment is wrong, it's async methods
     * --------------------------------------------------------- */

    /// <summary>
    /// Internal method to convert a file asynchronously.
    /// Validates file existence and prepares input for the worker.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="config">The configuration to use.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The Markdown result.</returns>
    /// <exception cref="MarkItDownConversionException">Thrown if file not found.</exception>
    private async Task<string> ConvertInternalAsync(string filePath, MarkDownConfig? config, CancellationToken ct)
    {
        if (!File.Exists(filePath))
            throw new MarkItDownConversionException($"File not found: '{filePath}'.");

        var input = new
        {
            Type = "file",
            Path = filePath,
            Kwargs = BuildKwargsDict(config)
        };

        return await RunWorkerAsync(input, ct);
    }

    /// <summary>
    /// Internal method to convert a stream asynchronously.
    /// Copies stream to memory, encodes to base64, and prepares input for the worker.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="extension">The file extension.</param>
    /// <param name="config">The configuration to use.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The Markdown result.</returns>
    /// <exception cref="MarkItDownConversionException">Thrown if stream is too large.</exception>
    private async Task<string> ConvertInternalAsync(Stream stream, string extension, MarkDownConfig? config, CancellationToken ct)
    {
        if (stream.Length > MaxStreamSizeBytes)
            throw new MarkItDownConversionException("Stream too large.");

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var data = System.Convert.ToBase64String(ms.ToArray());

        var input = new
        {
            Type = "stream",
            Data = data,
            Extension = extension,
            Kwargs = BuildKwargsDict(config)
        };

        return await RunWorkerAsync(input, ct);
    }

    /// <summary>
    /// Internal method to convert a URI asynchronously.
    /// Prepares input for the worker with URI details.
    /// </summary>
    /// <param name="uri">The URI to convert.</param>
    /// <param name="config">The configuration to use.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The Markdown result.</returns>
    private async Task<string> ConvertUriInternalAsync(string uri, MarkDownConfig? config, CancellationToken ct)
    {
        var input = new
        {
            Type = "uri",
            Uri = uri,
            Kwargs = BuildKwargsDict(config)
        };

        return await RunWorkerAsync(input, ct);
    }

    /* ---------------------------------------------------------
     * HELPERS
     * --------------------------------------------------------- */

    /// <summary>
    /// Builds a dictionary of keyword arguments from the configuration for passing to Python.
    /// Merges default and override configs, then maps to Python parameter names.
    /// </summary>
    /// <param name="config">The override configuration. If null, uses default.</param>
    /// <returns>A dictionary of kwargs for the Python markitdown function.</returns>
    private Dictionary<string, object> BuildKwargsDict(MarkDownConfig? config)
    {
        var mergedConfig = MergeConfigs(_defaultConfig, config);
        var kwargs = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(mergedConfig.DocIntel?.Endpoint))
            kwargs["docintel_endpoint"] = mergedConfig.DocIntel.Endpoint;

        if (!string.IsNullOrEmpty(mergedConfig.DocIntel?.Key))
            kwargs["docintel_key"] = mergedConfig.DocIntel.Key;

        if (!string.IsNullOrEmpty(mergedConfig.OpenAI?.ApiKey))
            kwargs["openai_api_key"] = mergedConfig.OpenAI.ApiKey;

        if (!string.IsNullOrEmpty(mergedConfig.OpenAI?.Model))
            kwargs["llm_model"] = mergedConfig.OpenAI.Model;

        if (!string.IsNullOrEmpty(mergedConfig.LlmPrompt))
            kwargs["llm_prompt"] = mergedConfig.LlmPrompt;

        if (mergedConfig.KeepDataUris == true)
            kwargs["keep_data_uris"] = true;

        if (mergedConfig.EnablePlugins == true)
            kwargs["enable_plugins"] = true;

        return kwargs;
    }

    /// <summary>
    /// Merges two configurations, with overrideConfig taking precedence over baseConfig.
    /// Null or empty values in overrideConfig fall back to baseConfig.
    /// </summary>
    /// <param name="baseConfig">The base configuration.</param>
    /// <param name="overrideConfig">The override configuration. Can be null.</param>
    /// <returns>The merged configuration.</returns>
    private static MarkDownConfig MergeConfigs(MarkDownConfig baseConfig, MarkDownConfig? overrideConfig)
    {
        if (overrideConfig == null)
            return baseConfig;

        return new MarkDownConfig
        {
            DocIntel = overrideConfig.DocIntel != null ? overrideConfig.DocIntel : baseConfig.DocIntel,
            OpenAI = overrideConfig.OpenAI != null ? overrideConfig.OpenAI : baseConfig.OpenAI,
            LlmPrompt = !string.IsNullOrEmpty(overrideConfig.LlmPrompt) ? overrideConfig.LlmPrompt : baseConfig.LlmPrompt,
            KeepDataUris = overrideConfig.KeepDataUris ?? baseConfig.KeepDataUris,
            EnablePlugins = overrideConfig.EnablePlugins ?? baseConfig.EnablePlugins,
            Plugins = overrideConfig.Plugins.Count > 0 ? overrideConfig.Plugins : baseConfig.Plugins
        };
    }
}