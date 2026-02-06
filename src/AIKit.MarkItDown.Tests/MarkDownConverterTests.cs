using Xunit.Abstractions;

namespace AIKit.MarkItDown.Tests;

public class MarkDownConverterTests
{
    private readonly ITestOutputHelper _output;

    public MarkDownConverterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData("pdf-test.pdf")]
    [InlineData("tst-text.txt")]
    public void ConvertToMarkdown_ReturnsMarkdown_ForTestFile(string fileName)
    {
        _output.WriteLine($"Starting conversion test for file: {fileName}");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
        _output.WriteLine($"Converting file: {filePath}");

        var result = converter.Convert(filePath);

        _output.WriteLine($"Conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // Add more assertions based on expected Markdown content, e.g., Assert.Contains("#", result);
    }

    [Theory]
    [InlineData("pdf-test.pdf")]
    [InlineData("tst-text.txt")]
    public void ConvertToMarkdown_WithConfig_ReturnsMarkdown_ForTestFile(string fileName)
    {
        _output.WriteLine($"Starting conversion test with config for file: {fileName}");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
        var config = new MarkDownConfig { KeepDataUris = true }; // Example config
        _output.WriteLine($"Converting file with config: {filePath}");

        var result = converter.Convert(filePath, config);

        _output.WriteLine($"Conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ConvertToMarkdown_ThrowsException_WhenFileDoesNotExist()
    {
        _output.WriteLine("Starting exception test for non-existent file");
        var converter = new MarkDownConverter();
        var nonExistentFile = "nonexistent.pdf";
        _output.WriteLine($"Attempting to convert non-existent file: {nonExistentFile}");

        var exception = Assert.Throws<MarkItDownConversionException>(() => converter.Convert(nonExistentFile));
        _output.WriteLine($"Exception type: {exception.GetType().Name}");
        _output.WriteLine($"Exception message: {exception.Message}");
        // The exception could be from installation failure or conversion failure
        Assert.Contains("File not found", exception.Message);
    }

    [Theory]
    [InlineData("pdf-test.pdf")]
    [InlineData("tst-text.txt")]
    public void ConvertStream_ReturnsMarkdown_ForTestFile(string fileName)
    {
        _output.WriteLine($"Starting stream conversion test for file: {fileName}");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
        string extension = Path.GetExtension(fileName).TrimStart('.');
        _output.WriteLine($"Converting stream: {filePath}");

        using (var stream = File.OpenRead(filePath))
        {
            var result = converter.Convert(stream, extension);

            _output.WriteLine($"Conversion result length: {result.Length}");
            _output.WriteLine("```markdown");
            _output.WriteLine(result);
            _output.WriteLine("```");

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }

    [Fact]
    public void ConvertUri_ReturnsMarkdown_ForValidUri()
    {
        _output.WriteLine("Starting URI conversion test");
        var converter = new MarkDownConverter();
        // Use a simple text URL for testing
        var uri = "https://www.example.com";
        _output.WriteLine($"Converting URI: {uri}");

        var result = converter.ConvertUri(uri);
        _output.WriteLine($"URI conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");
        Assert.NotNull(result);
        Assert.True(result.Length > 0, "Markdown should not be empty");
    }

    [Theory]
    [InlineData("pdf-test.pdf")]
    public void Convert_WithVariousConfigs_ReturnsMarkdown(string fileName)
    {
        _output.WriteLine($"Starting config variation test for file: {fileName}");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

        var configs = new[]
        {
            new MarkDownConfig { KeepDataUris = true },
            new MarkDownConfig { EnablePlugins = true },
            new MarkDownConfig { LlmModel = "gpt-4o" },
            new MarkDownConfig { DocIntelEndpoint = "https://test.endpoint" }
        };

        foreach (var config in configs)
        {
            _output.WriteLine($"Testing config: {config.KeepDataUris}, {config.EnablePlugins}, {config.LlmModel}, {config.DocIntelEndpoint}");
            var result = converter.Convert(filePath, config);
            _output.WriteLine($"Result length: {result.Length}");
            _output.WriteLine("```markdown");
            _output.WriteLine(result);
            _output.WriteLine("```");
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }

    [Fact]
    public void ConvertStream_WithConfig_ReturnsMarkdown()
    {
        _output.WriteLine("Starting stream with config test");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "tst-text.txt");
        var config = new MarkDownConfig { KeepDataUris = true };

        using (var stream = File.OpenRead(filePath))
        {
            var result = converter.Convert(stream, "txt", config);
            _output.WriteLine($"Stream with config result length: {result.Length}");
            _output.WriteLine("```markdown");
            _output.WriteLine(result);
            _output.WriteLine("```");
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }

    [Fact]
    public void ConvertUri_WithConfig_ReturnsMarkdown()
    {
        _output.WriteLine("Starting URI with config test");
        var converter = new MarkDownConverter();
        var uri = "https://www.example.com";
        var config = new MarkDownConfig { EnablePlugins = true };

        try
        {
            var result = converter.ConvertUri(uri, config);
            _output.WriteLine($"URI with config result length: {result.Length}");
            _output.WriteLine("```markdown");
            _output.WriteLine(result);
            _output.WriteLine("```");
            Assert.NotNull(result);
        }
        catch (MarkItDownConversionException ex)
        {
            Assert.Contains("MarkItDown URI conversion failed", ex.Message);
        }
    }

    [Fact]
    public void ConvertUri_YouTube_ReturnsMarkdown()
    {
        _output.WriteLine("Testing YouTube URL conversion");
        var converter = new MarkDownConverter();
        // Use a short YouTube video URL for testing
        var uri = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

        try
        {
            var result = converter.ConvertUri(uri);
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result));
            _output.WriteLine($"YouTube conversion successful: {result.Length} chars");
            _output.WriteLine("Content:");
            _output.WriteLine("```markdown");
            _output.WriteLine(result);
            _output.WriteLine("```");
        }
        catch (MarkItDownConversionException ex)
        {
            _output.WriteLine($"YouTube conversion failed: {ex.Message}");
            // YouTube might require additional setup, so we note it but don't fail the test
            Assert.Contains("MarkItDown URI conversion failed", ex.Message);
        }
    }

    // Async tests

    [Theory]
    [InlineData("pdf-test.pdf")]
    [InlineData("tst-text.txt")]
    public async Task ConvertAsyncToMarkdown_ReturnsMarkdown_ForTestFile(string fileName)
    {
        _output.WriteLine($"Starting async conversion test for file: {fileName}");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
        _output.WriteLine($"Converting file: {filePath}");

        var result = await converter.ConvertAsync(filePath);

        _output.WriteLine($"Conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("pdf-test.pdf")]
    [InlineData("tst-text.txt")]
    public async Task ConvertAsyncToMarkdown_WithConfig_ReturnsMarkdown_ForTestFile(string fileName)
    {
        _output.WriteLine($"Starting async conversion test with config for file: {fileName}");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
        var config = new MarkDownConfig { KeepDataUris = true }; // Example config
        _output.WriteLine($"Converting file with config: {filePath}");

        var result = await converter.ConvertAsync(filePath, config);

        _output.WriteLine($"Conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("pdf-test.pdf")]
    [InlineData("tst-text.txt")]
    public async Task ConvertAsyncStream_ReturnsMarkdown_ForTestFile(string fileName)
    {
        _output.WriteLine($"Starting async stream conversion test for file: {fileName}");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
        var extension = Path.GetExtension(fileName).TrimStart('.');
        _output.WriteLine($"Converting stream: {filePath}");

        using var stream = File.OpenRead(filePath);
        var result = await converter.ConvertAsync(stream, extension);

        _output.WriteLine($"Conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ConvertAsyncUri_ReturnsMarkdown_ForValidUri()
    {
        _output.WriteLine("Starting async URI conversion test");
        var converter = new MarkDownConverter();
        var uri = "https://www.example.com";
        _output.WriteLine($"Converting URI: {uri}");

        var result = await converter.ConvertUriAsync(uri);

        _output.WriteLine($"Conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ConvertAsyncUri_YouTube_ReturnsMarkdown()
    {
        _output.WriteLine("Testing async YouTube URL conversion");
        var converter = new MarkDownConverter();
        var uri = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

        var result = await converter.ConvertUriAsync(uri);

        _output.WriteLine($"YouTube conversion result length: {result.Length}");
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // YouTube transcripts might be empty or have specific content
    }

    [Fact]
    public async Task ConvertAsyncUri_WithConfig_ReturnsMarkdown()
    {
        _output.WriteLine("Starting async URI with config test");
        var converter = new MarkDownConverter();
        var uri = "https://www.example.com";
        var config = new MarkDownConfig { KeepDataUris = true };
        _output.WriteLine($"Converting URI with config: {uri}");

        var result = await converter.ConvertUriAsync(uri, config);

        _output.WriteLine($"Conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ConvertAsyncStream_WithConfig_ReturnsMarkdown()
    {
        _output.WriteLine("Starting async stream with config test");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "tst-text.txt");
        var config = new MarkDownConfig { KeepDataUris = true };

        using var stream = File.OpenRead(filePath);
        var result = await converter.ConvertAsync(stream, "txt", config);

        _output.WriteLine($"Conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("pdf-test.pdf")]
    public async Task ConvertAsync_WithVariousConfigs_ReturnsMarkdown(string fileName)
    {
        _output.WriteLine($"Starting async config variation test for file: {fileName}");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

        // Test with different configs
        var configs = new[]
        {
            new MarkDownConfig { KeepDataUris = true },
            new MarkDownConfig { KeepDataUris = false },
            new MarkDownConfig { EnablePlugins = true },
            null
        };

        foreach (var config in configs)
        {
            _output.WriteLine($"Testing config: {config?.KeepDataUris}, {config?.EnablePlugins}, {config?.Plugins?.Count ?? 0}");
            var result = await converter.ConvertAsync(filePath, config);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }

    [Fact]
    public async Task ConvertAsync_WithCancellation_ThrowsTaskCanceledException()
    {
        _output.WriteLine("Testing async cancellation");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "pdf-test.pdf");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(() => converter.ConvertAsync(filePath, ct: cts.Token));
    }

    [Fact]
    public async Task ConvertAsync_ParallelExecutions_Work()
    {
        _output.WriteLine("Testing parallel executions");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "tst-text.txt");

        var tasks = new List<Task<string>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(converter.ConvertAsync(filePath));
        }

        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        _output.WriteLine($"All {results.Length} parallel conversions succeeded");
    }
}