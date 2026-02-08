using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace AIKit.MarkItDown.Client.Tests;

/// <summary>
/// End-to-end tests for API key authentication.
/// </summary>
public class ApiKeyE2eTests : IAsyncLifetime
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
    /// Test API key.
    /// </summary>
    private const string TestApiKey = "12345";

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyE2eTests"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public ApiKeyE2eTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Initializes the test environment by starting the server with API key and client.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        var envVars = new Dictionary<string, string>
        {
            ["API_KEY"] = TestApiKey
        };
        await StartServerAsync(envVars);
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

    [Fact]
    public async Task ConvertFile_WithValidApiKey_ShouldSucceed()
    {
        // Arrange
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.txt");
        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

        // Set valid API key in client
        _httpClient!.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", TestApiKey);

        // Act
        var markdown = await _client!.ConvertAsync(filePath);
        _output.WriteLine($"Markdown response: {markdown}");

        // Assert
        Assert.NotNull(markdown);
        Assert.True(markdown.Length > 0, "Markdown content should not be empty");
    }

    [Fact]
    public async Task ConvertFile_WithInvalidApiKey_ShouldFail()
    {
        // Arrange
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.txt");
        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

        // Set invalid API key in client
        _httpClient!.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", "invalid-key");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _client!.ConvertAsync(filePath));
        Assert.Contains("Unauthorized", exception.Message);
    }

    [Fact]
    public async Task ConvertFile_WithoutApiKey_ShouldFail()
    {
        // Arrange
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.txt");
        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

        // Clear headers (no API key)
        _httpClient!.DefaultRequestHeaders.Clear();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _client!.ConvertAsync(filePath));
        Assert.Contains("Unauthorized", exception.Message);
    }

    [Fact]
    public async Task ConvertUri_WithValidApiKey_ShouldSucceed()
    {
        // Arrange
        var uri = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

        // Set valid API key in client
        _httpClient!.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", TestApiKey);

        // Act
        var markdown = await _client!.ConvertUriAsync(uri);
        _output.WriteLine($"Markdown response: {markdown}");

        // Assert
        Assert.NotNull(markdown);
        Assert.True(markdown.Length > 0, "Markdown content should not be empty");
    }

    private async Task StartServerAsync(Dictionary<string, string>? environmentVariables = null)
    {
        var apiDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AIKit.MarkItDown.Server");
        _uvicornProcess = StartUvicornServer(apiDir, environmentVariables);
        _output.WriteLine("Uvicorn server started with API key.");
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

    private Process StartUvicornServer(string apiDir, Dictionary<string, string>? environmentVariables = null)
    {
        string pythonExe;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            pythonExe = Path.Combine(apiDir, ".venv", "Scripts", "python.exe");
        }
        else
        {
            pythonExe = Path.Combine(apiDir, ".venv", "bin", "python");
        }

        if (!File.Exists(pythonExe))
        {
            throw new FileNotFoundException($"Python executable not found at {pythonExe}. Ensure the virtual environment is set up.");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = $"-m uvicorn main:app --host 0.0.0.0 --port {ServerPort}",
            WorkingDirectory = apiDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (environmentVariables != null)
        {
            foreach (var kvp in environmentVariables)
            {
                startInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
            }
        }

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
                // Server not ready yet
            }

            await Task.Delay(1000);
        }

        throw new Exception("Server did not become ready within the expected time.");
    }
}