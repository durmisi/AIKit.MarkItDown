using AIKit.MarkItDown;

Console.WriteLine("AIKit.MarkItDown Console Demo");
Console.WriteLine("=============================");

var testFilesDir = Path.Combine(AppContext.BaseDirectory, "test-files");
if (!Directory.Exists(testFilesDir))
{
    Console.WriteLine($"Test files directory not found: {testFilesDir}");
    return;
}

var converter = new MarkDownConverter();
var config = CreateConfig();

var files = Directory.GetFiles(testFilesDir, "*.*", SearchOption.TopDirectoryOnly)
    .Where(f => new[] { ".pdf", ".docx", ".html", ".txt" }.Contains(Path.GetExtension(f).ToLower()))
    .ToArray();

if (files.Length == 0)
{
    Console.WriteLine("No test files found.");
    return;
}

foreach (var file in files)
{
    Console.WriteLine($"\nConverting: {Path.GetFileName(file)}");
    Console.WriteLine(new string('-', 40));

    try
    {
        var result = converter.Convert(file, config);
        Console.WriteLine("Conversion successful!");
        Console.WriteLine("Markdown output:");
        Console.WriteLine(result);
    }
    catch (MarkItDownConversionException ex)
    {
        Console.WriteLine($"Conversion failed: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
    }
}

Console.WriteLine("\nDemo completed.");

static MarkDownConfig? CreateConfig()
{
    var config = new MarkDownConfig();

    // Check for Azure Document Intelligence
    var azureEndpoint = Environment.GetEnvironmentVariable("AZURE_DOC_INTELLIGENCE_ENDPOINT");
    var azureKey = Environment.GetEnvironmentVariable("AZURE_DOC_INTELLIGENCE_KEY");
    if (!string.IsNullOrEmpty(azureEndpoint) && !string.IsNullOrEmpty(azureKey))
    {
        config.DocIntel = new DocIntelConfig
        {
            Endpoint = azureEndpoint,
            Key = azureKey
        };
        Console.WriteLine("Azure Document Intelligence configured.");
    }

    // Check for OpenAI
    var openaiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    if (!string.IsNullOrEmpty(openaiKey))
    {
        config.OpenAI = new OpenAIConfig
        {
            ApiKey = openaiKey,
            Model = "gpt-4o"
        };
        Console.WriteLine("OpenAI configured.");
    }

    return config.DocIntel != null || config.OpenAI != null ? config : null;
}
