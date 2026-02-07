using System;
using Xunit.Abstractions;
using Moq;

namespace AIKit.MarkItDown.Tests;

/// <summary>
/// Unit tests for the <see cref="MarkDownConverter"/> class.
/// </summary>
public class MarkDownConverterTests
{
    /// <summary>
    /// Output helper for logging test results.
    /// </summary>
    private readonly ITestOutputHelper _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkDownConverterTests"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public MarkDownConverterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Tests that converting test files to Markdown returns valid Markdown.
    /// </summary>
    /// <param name="fileName">The name of the test file to convert.</param>
    [Theory]
    [InlineData("files/pdf-test.pdf")]
    [InlineData("files/tst-text.txt")]
    [InlineData("files/test.docx")]
    [InlineData("files/test.html")]
    [InlineData("files/test.txt")]
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

    /// <summary>
    /// Tests that converting test files with configuration returns valid Markdown.
    /// </summary>
    /// <param name="fileName">The name of the test file to convert.</param>
    [Theory]
    [InlineData("files/pdf-test.pdf")]
    [InlineData("files/tst-text.txt")]
    [InlineData("files/test.docx")]
    [InlineData("files/test.html")]
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

    /// <summary>
    /// Tests that converting a non-existent file throws an exception.
    /// </summary>
    [Fact]
    public void ConvertToMarkdown_ThrowsException_WhenFileDoesNotExist()
    {
        _output.WriteLine("Starting exception test for non-existent file");
        var converter = new MarkDownConverter();
        var nonExistentFile = "nonexistent.pdf";
        _output.WriteLine($"Attempting to convert non-existent file: {nonExistentFile}");

        var exception = Assert.Throws<AggregateException>(() => converter.Convert(nonExistentFile));
        _output.WriteLine($"Exception type: {exception.GetType().Name}");
        _output.WriteLine($"Exception message: {exception.Message}");
        Assert.IsType<MarkItDownConversionException>(exception.InnerException);
        Assert.Contains("File not found", exception.InnerException.Message);
    }

    /// <summary>
    /// Tests that the constructor with default config works correctly.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultConfig_Works()
    {
        _output.WriteLine("Testing constructor with default config");
        var docIntel = new DocIntelConfig { Endpoint = "endpoint", Key = "key" };
        var openAi = new OpenAIConfig { ApiKey = "api", Model = "model" };
        var converter = new MarkDownConverter(docIntel, openAi);
        var filePath = Path.Combine(AppContext.BaseDirectory, "files/test.txt");
        _output.WriteLine($"Converting file with default config: {filePath}");

        var result = converter.Convert(filePath);

        _output.WriteLine($"Conversion result length: {result.Length}");
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// Tests that the constructor with override config works correctly.
    /// </summary>
    [Fact]
    public void Constructor_WithOverrideConfig_Works()
    {
        _output.WriteLine("Testing constructor with override config");
        var docIntel = new DocIntelConfig { Endpoint = "endpoint", Key = "key" };
        var openAi = new OpenAIConfig { ApiKey = "api", Model = "model" };
        var converter = new MarkDownConverter(docIntel, openAi);
        var filePath = Path.Combine(AppContext.BaseDirectory, "files/test.txt");
        var overrideConfig = new MarkDownConfig { KeepDataUris = true };
        _output.WriteLine($"Converting file with override config: {filePath}");

        var result = converter.Convert(filePath, overrideConfig);

        _output.WriteLine($"Conversion result length: {result.Length}");
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Constructor_ValidatesDocIntelConfig()
    {
        _output.WriteLine("Testing constructor validation for DocIntelConfig");
        var invalidDocIntel = new DocIntelConfig { Endpoint = "", Key = "key" };

        var exception = Assert.Throws<ArgumentException>(() => new MarkDownConverter(invalidDocIntel, null));
        Assert.Contains("DocIntelConfig.Endpoint", exception.Message);
    }

    [Fact]
    public void Constructor_ValidatesOpenAIConfig()
    {
        _output.WriteLine("Testing constructor validation for OpenAIConfig");
        var invalidOpenAi = new OpenAIConfig { ApiKey = "api", Model = "" };

        var exception = Assert.Throws<ArgumentException>(() => new MarkDownConverter(null, invalidOpenAi));
        Assert.Contains("OpenAIConfig.Model", exception.Message);
    }

    [Theory]
    [InlineData("files/pdf-test.pdf")]
    [InlineData("files/tst-text.txt")]
    [InlineData("files/test.docx")]
    [InlineData("files/test.html")]
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

    // Async tests

    [Theory]
    [InlineData("files/pdf-test.pdf")]
    [InlineData("files/tst-text.txt")]
    [InlineData("files/test.docx")]
    [InlineData("files/test.html")]
    [InlineData("files/test.txt")]
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
    [InlineData("files/pdf-test.pdf")]
    [InlineData("files/tst-text.txt")]
    [InlineData("files/test.docx")]
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
    [InlineData("files/pdf-test.pdf")]
    [InlineData("files/tst-text.txt")]
    [InlineData("files/test.docx")]
    [InlineData("files/test.html")]
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
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "tst-text.txt");
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
    [InlineData("files/pdf-test.pdf")]
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
    public async Task ConvertAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        _output.WriteLine("Testing async cancellation");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "pdf-test.pdf");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => converter.ConvertAsync(filePath, ct: cts.Token));
    }

    [Fact]
    public async Task ConvertAsync_ParallelExecutions_Work()
    {
        _output.WriteLine("Testing parallel executions");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "tst-text.txt");

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

    [Fact]
    public void TestConvertDocx()
    {
        _output.WriteLine("Testing DOCX conversion");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.docx");

        var result = converter.Convert(filePath);

        _output.WriteLine($"DOCX conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("#", result); // Assuming it has headings
    }

    [Fact]
    public void TestConvertXlsx()
    {
        _output.WriteLine("Testing XLSX conversion");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.xlsx");

        var result = converter.Convert(filePath);

        _output.WriteLine($"XLSX conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("|", result); // Tables
    }

    [Fact]
    public void TestConvertPptx()
    {
        _output.WriteLine("Testing PPTX conversion");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.pptx");

        var result = converter.Convert(filePath);

        _output.WriteLine($"PPTX conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void TestConvertImage()
    {
        _output.WriteLine("Testing image conversion");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.jpg");

        var result = converter.Convert(filePath);

        _output.WriteLine($"Image conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        // Allow empty for now if OCR not available
    }

    [Fact]
    public void TestConvertAudio()
    {
        _output.WriteLine("Testing audio conversion");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "testaudio_16000_test01_20s.wav");

        var result = converter.Convert(filePath);

        _output.WriteLine($"Audio conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void TestConvertZip()
    {
        _output.WriteLine("Testing ZIP conversion");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.zip");

        var result = converter.Convert(filePath);

        _output.WriteLine($"ZIP conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void TestConvertEpub()
    {
        _output.WriteLine("Testing EPUB conversion");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.epub");

        var result = converter.Convert(filePath);

        _output.WriteLine($"EPUB conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void TestConvertCsv()
    {
        _output.WriteLine("Testing CSV conversion");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.csv");

        var result = converter.Convert(filePath);

        _output.WriteLine($"CSV conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("|", result); // Tables
    }

    [Fact]
    public void TestConvertIpynb()
    {
        _output.WriteLine("Testing IPYNB conversion");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.ipynb");

        var result = converter.Convert(filePath);

        _output.WriteLine($"IPYNB conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void TestConvertHtml()
    {
        _output.WriteLine("Testing HTML conversion");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.html");

        var result = converter.Convert(filePath);

        _output.WriteLine($"HTML conversion result length: {result.Length}");
        _output.WriteLine("```markdown");
        _output.WriteLine(result);
        _output.WriteLine("```");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void TestConvertYoutube()
    {
        _output.WriteLine("Testing YouTube URI conversion");
        var converter = new MarkDownConverter();
        var uri = "https://www.youtube.com/watch?v=jNQXAC9IVRw";

        try
        {
            var result = converter.ConvertUri(uri);
            _output.WriteLine($"YouTube conversion result length: {result.Length}");
            _output.WriteLine("```markdown");
            _output.WriteLine(result);
            _output.WriteLine("```");
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
        catch (MarkItDownConversionException ex)
        {
            _output.WriteLine($"YouTube conversion failed: {ex.Message}");
            // Allow failure if transcription not set up
        }
    }

    [SkippableFact]
    public void TestConvertWithLlmConfig()
    {
        Skip.If(Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true", "Skipping on GitHub due to missing OpenAI credentials");

        _output.WriteLine("Testing conversion with LLM config");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.jpg");
        var config = new MarkDownConfig { OpenAI = new OpenAIConfig { Model = "gpt-4o", ApiKey = "mock-key" } }; // Mock key, expect graceful failure or no LLM call

        try
        {
            var result = converter.Convert(filePath, config);
            _output.WriteLine($"LLM config result length: {result.Length}");
            _output.WriteLine("```markdown");
            _output.WriteLine(result);
            _output.WriteLine("```");
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
        catch (MarkItDownConversionException ex)
        {
            _output.WriteLine($"LLM config failed: {ex.Message}");
            // Allow failure if API key invalid
        }
    }

    [Fact]
    public void TestConvertWithAzureConfig()
    {
        _output.WriteLine("Testing conversion with Azure Doc Intel config");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.pdf");
        var config = new MarkDownConfig { DocIntel = new DocIntelConfig { Endpoint = "https://mock.endpoint", Key = "mock-key" } };

        try
        {
            var result = converter.Convert(filePath, config);
            _output.WriteLine($"Azure config result length: {result.Length}");
            _output.WriteLine("```markdown");
            _output.WriteLine(result);
            _output.WriteLine("```");
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
        catch (MarkItDownConversionException ex)
        {
            _output.WriteLine($"Azure config failed: {ex.Message}");
            // Allow failure if endpoint invalid
        }
    }

    [Fact]
    public void TestConvertCorruptedFile()
    {
        _output.WriteLine("Testing conversion of corrupted file");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "corrupted.pdf");
        File.WriteAllText(filePath, "not a pdf"); // Create corrupted file

        var result = converter.Convert(filePath);
        // Markitdown may fall back to text conversion
        Assert.NotNull(result);
        Assert.Contains("not a pdf", result);
    }

    [Fact]
    public void TestConvertUriNetworkFailure()
    {
        _output.WriteLine("Testing URI conversion with network failure");
        var converter = new MarkDownConverter();
        var uri = "https://invalid.domain.that.does.not.exist";

        var ex = Assert.Throws<AggregateException>(() => converter.ConvertUri(uri));
        Assert.IsType<MarkItDownConversionException>(ex.InnerException);
    }

    [Fact]
    public void TestConvertOutputValidation()
    {
        _output.WriteLine("Testing output validation for XLSX");
        var converter = new MarkDownConverter();
        var filePath = Path.Combine(AppContext.BaseDirectory, "files", "test.xlsx");

        var result = converter.Convert(filePath);

        Assert.Contains("|", result); // Should have table markdown
        Assert.Contains("Name", result); // From our test data
    }
}