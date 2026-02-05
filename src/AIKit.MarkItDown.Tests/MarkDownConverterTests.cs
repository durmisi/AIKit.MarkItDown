using AIKit.MarkItDown;
using Xunit.Abstractions;
using System.IO;

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
        _output.WriteLine(result);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // Add more assertions based on expected Markdown content, e.g., Assert.Contains("#", result);
    }

    [Fact]
    public void ConvertToMarkdown_ThrowsException_WhenFileDoesNotExist()
    {
        _output.WriteLine("Starting exception test for non-existent file");
        var converter = new MarkDownConverter();
        var nonExistentFile = "nonexistent.pdf";
        _output.WriteLine($"Attempting to convert non-existent file: {nonExistentFile}");

        var exception = Assert.Throws<Exception>(() => converter.Convert(nonExistentFile));
        _output.WriteLine($"Exception type: {exception.GetType().Name}");
        _output.WriteLine($"Exception message: {exception.Message}");
        // The exception could be from installation failure or conversion failure
        Assert.True(exception.Message.Contains("Failed to install") || exception.Message.Contains("MarkItDown conversion failed"));
    }
}
