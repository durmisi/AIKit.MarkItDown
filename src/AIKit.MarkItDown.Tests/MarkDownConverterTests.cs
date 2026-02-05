using AIKit.MarkItDown;
using Xunit.Abstractions;

namespace AIKit.MarkItDown.Tests;

public class MarkDownConverterTests
{
    private readonly ITestOutputHelper _output;

    public MarkDownConverterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConvertToMarkdown_ReturnsMarkdown_ForPdfTestFile()
    {
        var converter = new MarkDownConverter();
        var filePath = "pdf-test.pdf"; // Ensure this file exists in the test directory or provide a full path

        var result = converter.Convert(filePath);

        _output.WriteLine(result);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // Add more assertions based on expected Markdown content, e.g., Assert.Contains("#", result);
    }

    [Fact]
    public void ConvertToMarkdown_ThrowsException_WhenFileDoesNotExist()
    {
        var converter = new MarkDownConverter();
        var nonExistentFile = "nonexistent.pdf";

        var exception = Assert.Throws<Exception>(() => converter.Convert(nonExistentFile));
        _output.WriteLine(exception.Message);
        // The exception could be from installation failure or conversion failure
        Assert.True(exception.Message.Contains("Failed to install") || exception.Message.Contains("MarkItDown conversion failed"));
    }
}
