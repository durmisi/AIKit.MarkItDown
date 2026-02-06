using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace AIKit.MarkItDown.Client.Tests;

/// <summary>
/// End-to-end tests for the MarkItDown client against a running server.
/// </summary>
public class E2eTests : IAsyncLifetime
{
    /// <summary>
    /// Output helper for logging test results.
    /// </summary>
    private readonly ITestOutputHelper _output;

    /// <summary>
    /// The Uvicorn process running the server.
    /// </summary>
    private Process? _uvicornProcess;

    /// <summary>
    /// The HTTP client for making requests.
    /// </summary>
    private HttpClient? _httpClient;

    /// <summary>
    /// The MarkItDown client instance.
    /// </summary>
    private MarkItDownClient? _client;

    /// <summary>
    /// The URL of the test server.
    /// </summary>
    private const string ServerUrl = "http://localhost:8000";

    /// <summary>
    /// The health endpoint for checking server status.
    /// </summary>
    private const string HealthEndpoint = "/health";

    /// <summary>
    /// The port on which the server runs.
    /// </summary>
    private const int ServerPort = 8000;

    /// <summary>
    /// Maximum number of retries for server readiness.
    /// </summary>
    private const int MaxRetries = 30;

    /// <summary>
    /// Initializes a new instance of the <see cref="E2eTests"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public E2eTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Initializes the test environment by starting the server and client.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        await StartServerAsync();
        await WaitForServerReadyAsync();
        InitializeClient();
    }

    /// <summary>
    /// Disposes the test environment by stopping the server and disposing the client.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DisposeAsync()
    {
        await StopServerAsync();
        _httpClient?.Dispose();
    }

    /// <summary>
    /// Tests converting various file types to Markdown using the client.
    /// </summary>
    /// <param name="fileName">The name of the test file to convert.</param>
    [Theory]
    [InlineData("files/pdf-test.pdf")]
    [InlineData("files/test.csv")]
    [InlineData("files/test.docx")]
    [InlineData("files/test.epub")]
    [InlineData("files/test.html")]
    [InlineData("files/test.ipynb")]
    [InlineData("files/test.pdf")]
    [InlineData("files/test.pptx")]
    [InlineData("files/test.txt")]
    [InlineData("files/test.xlsx")]
    [InlineData("files/test.zip")]
    [InlineData("files/tst-text.txt")]
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
        string testFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "TestShared", "files", "tst-text.txt"));
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
            FileName = "python",
            Arguments = $"-m uvicorn main:app --host 0.0.0.0 --port {ServerPort}",
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
        var baseDir = Path.GetDirectoryName(typeof(E2eTests).Assembly.Location)!;
        var workerProjectDir = Path.Combine(baseDir, "..", "..", "..", "..", "AIKit.MarkItDown.Worker", "bin");
        
        string exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "AIKit.MarkItDown.Worker.exe" : "AIKit.MarkItDown.Worker";
        
        // Try Debug first, then Release
        var configurations = new[] { "Debug", "Release" };
        string? workerBinDir = null;
        foreach (var config in configurations)
        {
            var dir = Path.Combine(workerProjectDir, config, "net10.0");
            if (Directory.Exists(dir) && File.Exists(Path.Combine(dir, exeName)))
            {
                workerBinDir = dir;
                break;
            }
        }
        
        if (workerBinDir == null)
        {
            throw new FileNotFoundException($"Could not find {exeName} in Debug or Release directories.");
        }
        
        var exePath = Path.Combine(workerBinDir, exeName);
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