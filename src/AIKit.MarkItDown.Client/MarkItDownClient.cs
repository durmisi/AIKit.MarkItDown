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

    public async Task<FileConversionResponse> ConvertAsync(Stream fileStream, string fileName)
    {
        _logger.LogInformation("Starting file conversion for {FileName}", fileName);
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);
        var response = await _httpClient.PostAsync("/convert", content);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<FileConversionResponse>() ?? new FileConversionResponse();
        _logger.LogInformation("File conversion completed for {FileName}, result filename: {ResultFilename}", fileName, result.Filename);
        return result;
    }

    public async Task<FileConversionResponse> ConvertAsync(string filePath)
    {
        _logger.LogInformation("Converting file from path {FilePath}", filePath);
        using var stream = File.OpenRead(filePath);
        string fileName = Path.GetFileName(filePath);
        return await ConvertAsync(stream, fileName);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
