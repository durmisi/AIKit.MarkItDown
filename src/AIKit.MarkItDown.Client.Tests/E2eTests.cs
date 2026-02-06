using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace AIKit.MarkItDown.Client.Tests;

public class E2eTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private Process? _uvicornProcess;
    private HttpClient? _httpClient;
    private MarkItDownClient? _client;

    private const string ServerUrl = "http://localhost:8000";
    private const string HealthEndpoint = "/health";
    private const int ServerPort = 8000;
    private const int MaxRetries = 30;

    public E2eTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        await StartServerAsync();
        await WaitForServerReadyAsync();
        InitializeClient();
    }

    public async Task DisposeAsync()
    {
        await StopServerAsync();
        _httpClient?.Dispose();
    }

    [Theory]
    [InlineData("pdf-test.pdf")]
    [InlineData("tst-text.txt")]
    public async Task UploadFileAndVerifyMarkdown(string fileName)
    {
        // Arrange
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

        _output.WriteLine($"Testing file conversion with file: {filePath}");

        // Act
        var markdown = await _client!.ConvertAsync(filePath);

        // Assert
        Assert.NotNull(markdown);
        Assert.True(markdown.Length > 0, "Markdown content should not be empty");
        _output.WriteLine($@"Response received:
            Markdown length={markdown.Length},
            Content preview={markdown.Substring(0, Math.Min(100, markdown.Length))}
        ");
    }

    [Fact]
    public async Task ConvertTextFileViaWorker_ShouldReturnMarkdown()
    {
        // Arrange
        string testFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "tst-text.txt"));
        var input = new
        {
            Type = "file",
            Path = testFilePath,
            Kwargs = new Dictionary<string, object>()
        };
        var jsonInput = JsonSerializer.Serialize(input);

        // Act
        var result = await RunWorker(jsonInput);

        _output.WriteLine($"Result: Success={result.Success}, Result length={result.Result?.Length}, Error={result.Error}");

        // Assert
        if (!result.Success)
        {
            throw new Exception($"Worker failed: {result.Error}");
        }
        Assert.True(result.Success);
        Assert.Contains("Test Document for MarkItDown", result.Result);
    }

    private async Task StartServerAsync()
    {
        var apiDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AIKit.MarkItDown.Server");
        _uvicornProcess = StartUvicornServer(apiDir);
        _output.WriteLine("Uvicorn server started.");
    }

    private async Task StopServerAsync()
    {
        if (_uvicornProcess != null && !_uvicornProcess.HasExited)
        {
            _uvicornProcess.Kill();
            await _uvicornProcess.WaitForExitAsync();
            _output.WriteLine("Uvicorn server stopped.");
        }
    }

    private void InitializeClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(ServerUrl),
            Timeout = TimeSpan.FromMinutes(5)
        };

        var logger = NullLogger<MarkItDownClient>.Instance;
        _client = new MarkItDownClient(_httpClient, logger);

        _output.WriteLine("HttpClient and MarkItDownClient initialized.");
    }

    private Process StartUvicornServer(string apiDir)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "uvicorn",
            Arguments = $"main:app --host 0.0.0.0 --port {ServerPort}",
            WorkingDirectory = apiDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(startInfo);
        return process ?? throw new InvalidOperationException("Failed to start Uvicorn process.");
    }

    private async Task WaitForServerReadyAsync()
    {
        _output.WriteLine("Waiting for server to be ready...");
        using var healthClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        for (int i = 0; i < MaxRetries; i++)
        {
            try
            {
                var response = await healthClient.GetAsync($"{ServerUrl}{HealthEndpoint}");
                if (response.IsSuccessStatusCode)
                {
                    _output.WriteLine("Server is ready.");
                    return;
                }
            }
            catch
            {
                // Ignore and retry
            }
            await Task.Delay(1000);
        }
        throw new TimeoutException("Server did not become ready within the expected time.");
    }

    private async Task<WorkerOutput> RunWorker(string inputJson)
    {
        var workerBinDir = Path.Combine(Path.GetDirectoryName(typeof(E2eTests).Assembly.Location)!, "..", "..", "..", "..", "AIKit.MarkItDown.Worker", "bin", "Debug", "net10.0");
        var exePath = Path.Combine(workerBinDir, "AIKit.MarkItDown.Worker.exe");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = "",
                WorkingDirectory = workerBinDir,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        await process.StandardInput.WriteAsync(inputJson);
        process.StandardInput.Close();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0 || !string.IsNullOrEmpty(error))
        {
            throw new Exception($"Worker failed: exit code {process.ExitCode}, error: {error}, output: {output}");
        }

        _output.WriteLine($"Worker output: '{output.Trim()}'");
        output = output.Trim();
        var doc = JsonDocument.Parse(output);
        var root = doc.RootElement;
        var success = root.GetProperty("success").GetBoolean();
        var resultStr = root.TryGetProperty("result", out var resultProp) ? resultProp.GetString() : null;
        var errorStr = root.TryGetProperty("error", out var errorProp) ? errorProp.GetString() : null;
        var result = new WorkerOutput { Success = success, Result = resultStr, Error = errorStr };
        return result;
    }

    private class WorkerOutput
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("result")]
        public string? Result { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}