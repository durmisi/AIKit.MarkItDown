using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace AIKit.MarkItDown.Worker.Tests;

public class WorkerTests
{
    private readonly ITestOutputHelper _output;

    public WorkerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ConvertTextFile_ShouldReturnMarkdown()
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

    [Fact]
    public async Task ConvertStream_ShouldReturnMarkdown()
    {
        // Arrange
        var data = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello World from stream"));
        var input = new
        {
            Type = "stream",
            Data = data,
            Extension = ".txt",
            Kwargs = new Dictionary<string, object>()
        };
        var jsonInput = JsonSerializer.Serialize(input);

        // Act
        var result = await RunWorker(jsonInput);

        _output.WriteLine($"Result: Success={result.Success}, Result length={result.Result?.Length}, Error={result.Error}");

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Hello World from stream", result.Result);
    }

    [Fact]
    public async Task InvalidInput_ShouldReturnError()
    {
        // Arrange
        var jsonInput = "invalid json";

        // Act
        var result = await RunWorker(jsonInput);

        _output.WriteLine($"Result: Success={result.Success}, Result length={result.Result?.Length}, Error={result.Error}");

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    private async Task<WorkerOutput> RunWorker(string inputJson)
    {
        var workerBinDir = Path.Combine(Path.GetDirectoryName(typeof(WorkerTests).Assembly.Location)!, "..", "..", "..", "..", "AIKit.MarkItDown.Worker", "bin", "Debug", "net10.0");
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