using AIKit.MarkItDown.Client;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using Xunit;

namespace AIKit.MarkItDown.Tests;

public class E2eTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private Process? _uvicornProcess;
    private HttpClient? _httpClient;
    private MarkItDownClient? _client;

    private const string ServerUrl = "http://localhost:8000";
    private const string HealthEndpoint = "/health";
    private static readonly string[] TestFileNames = { "pdf-test.pdf", "tst-text.txt" };
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
        var response = await _client!.ConvertAsync(filePath);

        // Assert
        Assert.NotNull(response);
        _output.WriteLine($"Response received: Filename={response.Filename}, Markdown length={response.Markdown?.Length ?? 0}");
        Assert.Equal(fileName, response.Filename);
        Assert.NotNull(response.Markdown);
        Assert.NotEmpty(response.Markdown);
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
}