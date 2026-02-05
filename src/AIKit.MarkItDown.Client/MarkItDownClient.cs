using System.Net.Http.Json;
using System.IO;
using Microsoft.Extensions.Logging;

namespace AIKit.MarkItDown.Client;

public class MarkItDownClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MarkItDownClient> _logger;

    public MarkItDownClient(HttpClient httpClient, ILogger<MarkItDownClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> ConvertAsync(Stream fileStream, string fileName, string? extension = null, MarkDownConfig? config = null)
    {
        _logger.LogInformation("Starting file conversion for {FileName}", fileName);
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);
        if (extension != null)
        {
            content.Add(new StringContent(extension), "extension");
        }
        if (config != null)
        {
            content.Add(new StringContent(System.Text.Json.JsonSerializer.Serialize(config)), "config");
        }
        var response = await _httpClient.PostAsync("/convert", content);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("File conversion completed for {FileName}", fileName);
        return result;
    }

    public async Task<string> ConvertAsync(string filePath, string? extension = null, MarkDownConfig? config = null)
    {
        _logger.LogInformation("Converting file from path {FilePath}", filePath);
        using var stream = File.OpenRead(filePath);
        string fileName = Path.GetFileName(filePath);
        return await ConvertAsync(stream, fileName, extension, config);
    }

    public async Task<string> ConvertUriAsync(string uri, MarkDownConfig? config = null)
    {
        _logger.LogInformation("Converting URI {Uri}", uri);
        var request = new ConvertUriRequest { Uri = uri, Config = config };
        var response = await _httpClient.PostAsJsonAsync("/convert_uri", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("URI conversion completed for {Uri}", uri);
        return result;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
