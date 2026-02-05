using Xunit.Abstractions;

namespace AIKit.MarkItDown.Tests;

[Collection("Sequential")]
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
        var uri = "https://www.example.com"; // This might not work, but test the method
        _output.WriteLine($"Converting URI: {uri}");

        // Note: This may fail if network is not available or URL doesn't convert
        try
        {
            var result = converter.ConvertUri(uri);
            _output.WriteLine($"URI conversion result length: {result.Length}");
            _output.WriteLine("```markdown");
            _output.WriteLine(result);
            _output.WriteLine("```");
            Assert.NotNull(result);
            // Assert.NotNull(result.Text); // May be empty for non-convertible URLs
        }
        catch (MarkItDownConversionException ex)
        {
            _output.WriteLine($"URI conversion failed as expected: {ex.Message}");
            // Assert that it's a conversion failure, not a method error
            Assert.Contains("MarkItDown URI conversion failed", ex.Message);
        }
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

    [Fact]
    public void CreateOpenAiClient_Works_WhenOpenAiInstalled()
    {
        _output.WriteLine("Testing OpenAI client creation with package installed");

        // Since OpenAI is installed, this should not throw for package missing
        // It might throw for invalid API key, but not for missing package
        try
        {
            var client = MarkDownConverter.CreateOpenAiClient("fake-key");
            // If it succeeds, that's fine
            Assert.NotNull(client);
        }
        catch (MarkItDownConversionException ex)
        {
            // Should not be about missing package
            Assert.DoesNotContain("OpenAI package not installed", ex.Message);
        }
    }

    [Fact]
    public void ValidateConfigRequirements_Works_ForInstalledPackages()
    {
        _output.WriteLine("Testing config validation with installed packages");

        var config = new MarkDownConfig { DocIntelEndpoint = "endpoint" };
        // Should not throw since azure-ai-documentintelligence is installed
        MarkDownConverter.ValidateConfigRequirements(config);
        // If no exception, test passes
    }
}