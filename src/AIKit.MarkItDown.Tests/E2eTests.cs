using AIKit.MarkItDown.Client;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;

namespace AIKit.MarkItDown.Tests;

public class E2eTests
{
    private readonly ITestOutputHelper _output;
    private Process? _uvicornProcess;

    public E2eTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task UploadPdfAndVerifyMarkdown()
    {
        // Start the FastAPI server manually
        var apiDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AIKit.MarkItDown.Api");
        _uvicornProcess = StartUvicornServer(apiDir);
        
        try
        {
            _output.WriteLine("Uvicorn server started. Waiting for health check...");
            
            // Wait for server to be ready
            await WaitForServerReady("http://localhost:8000/health");
            _output.WriteLine("Server is ready.");
            
            // Create HttpClient (no SSL bypass needed for HTTP)
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8000"),
                Timeout = TimeSpan.FromMinutes(5)
            };

            _output.WriteLine("HttpClient created.");

            // Create logger
            var logger = NullLogger<MarkItDownClient>.Instance;

            // Instantiate the client
            var client = new MarkItDownClient(httpClient, logger);

            _output.WriteLine("MarkItDownClient instantiated.");

            // Path to the test PDF
            var pdfPath = Path.Combine(AppContext.BaseDirectory, "pdf-test.pdf");

            _output.WriteLine($"PDF path: {pdfPath}");

            // Convert the PDF
            _output.WriteLine("Starting PDF conversion...");
            try
            {
                var response = await client.ConvertAsync(pdfPath);

                _output.WriteLine("PDF conversion completed.");

                // Assertions
                Assert.NotNull(response);
                _output.WriteLine($"Response received: Filename={response.Filename}, Markdown length={response.Markdown?.Length ?? 0}");
                Assert.Equal("pdf-test.pdf", response.Filename);
                Assert.NotNull(response.Markdown);
                Assert.NotEmpty(response.Markdown);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error during conversion: {ex.Message}");
                _output.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                throw;
            }
        }
        finally
        {
            // Stop the server
            if (_uvicornProcess != null && !_uvicornProcess.HasExited)
            {
                _uvicornProcess.Kill();
                _uvicornProcess.WaitForExit();
                _output.WriteLine("Uvicorn server stopped.");
            }
        }
    }

    private Process StartUvicornServer(string apiDir)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "uvicorn",
            Arguments = "main:app --host 0.0.0.0 --port 8000",
            WorkingDirectory = apiDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start Uvicorn process.");
        }
        return process;
    }

    private async Task WaitForServerReady(string healthUrl, int maxRetries = 30)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await client.GetAsync(healthUrl);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // Ignore and retry
            }
            await Task.Delay(1000);
        }
        throw new TimeoutException("Server did not become ready.");
    }
}