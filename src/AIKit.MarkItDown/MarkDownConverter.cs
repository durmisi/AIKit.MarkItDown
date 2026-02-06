using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIKit.MarkItDown;

public class MarkDownConverter
{
    private const long MaxStreamSizeBytes = 100 * 1024 * 1024; // 100 MB
    private const int DefaultTimeoutMs = 30_000;
    private static readonly string WorkerExePath;

    static MarkDownConverter()
    {
        var assemblyDir = Path.GetDirectoryName(typeof(MarkDownConverter).Assembly.Location)!;
        WorkerExePath = Path.Combine(assemblyDir, "AIKit.MarkItDown.Worker.exe");
        if (!File.Exists(WorkerExePath))
            throw new InvalidOperationException("Worker exe not found.");
    }

    /* ---------------------------------------------------------
     * PUBLIC ASYNC API
     * --------------------------------------------------------- */

    public Task<string> ConvertAsync(
        string filePath,
        MarkDownConfig? config = null,
        CancellationToken ct = default)
        => RunAsync((ct) => ConvertInternalAsync(filePath, config, ct), ct);

    public Task<string> ConvertAsync(
        Stream stream,
        string extension,
        MarkDownConfig? config = null,
        CancellationToken ct = default)
        => RunAsync((ct) => ConvertInternalAsync(stream, extension, config, ct), ct);

    public Task<string> ConvertUriAsync(
        string uri,
        MarkDownConfig? config = null,
        CancellationToken ct = default)
        => RunAsync((ct) => ConvertUriInternalAsync(uri, config, ct), ct);

    /* ---------------------------------------------------------
     * PUBLIC SYNC API
     * --------------------------------------------------------- */

    public string Convert(
        string filePath,
        MarkDownConfig? config = null)
        => ConvertInternalAsync(filePath, config, default).Result;

    public string Convert(
        Stream stream,
        string extension,
        MarkDownConfig? config = null)
        => ConvertInternalAsync(stream, extension, config, default).Result;

    public string ConvertUri(
        string uri,
        MarkDownConfig? config = null)
        => ConvertUriInternalAsync(uri, config, default).Result;

    /* ---------------------------------------------------------
     * ASYNC EXECUTION WRAPPER
     * --------------------------------------------------------- */

    private static async Task<string> RunAsync(
        Func<CancellationToken, Task<string>> work,
        CancellationToken ct,
        int timeoutMs = DefaultTimeoutMs)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeoutMs);

        return await work(cts.Token);
    }

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

    private record WorkerResult(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("result")] string? Result,
        [property: JsonPropertyName("error")] string? Error);

    /* ---------------------------------------------------------
     * SYNC PYTHON CORE (GIL OWNERSHIP)
     * --------------------------------------------------------- */

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

    private static Dictionary<string, object> BuildKwargsDict(MarkDownConfig? config)
    {
        var kwargs = new Dictionary<string, object>();

        if (config == null)
            return kwargs;

        if (!string.IsNullOrEmpty(config.DocIntelEndpoint))
            kwargs["docintel_endpoint"] = config.DocIntelEndpoint;

        if (!string.IsNullOrEmpty(config.DocIntelKey))
            kwargs["docintel_key"] = config.DocIntelKey;

        if (!string.IsNullOrEmpty(config.OpenAiApiKey))
            kwargs["openai_api_key"] = config.OpenAiApiKey;

        if (!string.IsNullOrEmpty(config.LlmModel))
            kwargs["llm_model"] = config.LlmModel;

        if (!string.IsNullOrEmpty(config.LlmPrompt))
            kwargs["llm_prompt"] = config.LlmPrompt;

        if (config.KeepDataUris)
            kwargs["keep_data_uris"] = true;

        if (config.EnablePlugins)
            kwargs["enable_plugins"] = true;

        return kwargs;
    }
}