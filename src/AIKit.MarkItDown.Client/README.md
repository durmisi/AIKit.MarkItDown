# AIKit.MarkItDown.Client

This is the HTTP client library for AIKit.MarkItDown. It provides a simple way to convert various file formats to Markdown using the AIKit.MarkItDown service.

## Usage

### Dependency Injection (Recommended)

```csharp
using Microsoft.Extensions.DependencyInjection;
using AIKit.MarkItDown.Client;

// In your service registration
builder.Services.AddMarkItDownClient("http://localhost:8000", apiKey: "your-api-key");

// In your class
public class MyService
{
    private readonly MarkItDownClient _client;

    public MyService(MarkItDownClient client)
    {
        _client = client;
    }

    public async Task<string> ConvertFileAsync(string filePath)
    {
        return await _client.ConvertAsync(filePath);
    }

    public async Task<string> ConvertUriAsync(string uri)
    {
        return await _client.ConvertUriAsync(uri);
    }
}
```

### Manual Setup

```csharp
using AIKit.MarkItDown.Client;
using Microsoft.Extensions.Logging;

// Create HTTP client
var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8000") };
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<MarkItDownClient>();
var client = new MarkItDownClient(httpClient, logger);

// Convert a file
var markdown = await client.ConvertAsync("path/to/file.pdf");

// Convert a URI
var markdown = await client.ConvertUriAsync("https://example.com/document.pdf");
```

### With Configuration

```csharp
using AIKit.MarkItDown.Client;

var config = new MarkDownConfig
{
    // Configure DocIntel, OpenAI, etc. as needed
    KeepDataUris = true
};

var markdown = await client.ConvertAsync("path/to/file.pdf", config: config);
```

## Installation

Install via NuGet:

```
dotnet add package AIKit.MarkItDown.Client
```
