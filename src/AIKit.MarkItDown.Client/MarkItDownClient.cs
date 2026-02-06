using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace AIKit.MarkItDown.Client;

/// <summary>
/// Client for interacting with the MarkItDown server API.
/// Provides methods to convert files and URIs to Markdown.
/// </summary>
public class MarkItDownClient : IDisposable
{
    /// <summary>
    /// The HTTP client used for making requests.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// The logger for logging operations.
    /// </summary>
    private readonly ILogger<MarkItDownClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkItDownClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="logger">The logger for logging operations.</param>
    /// <exception cref="ArgumentNullException">Thrown if httpClient or logger is null.</exception>
    public MarkItDownClient(HttpClient httpClient, ILogger<MarkItDownClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Converts a file stream to Markdown asynchronously.
    /// Sends a multipart form request to the /convert endpoint.
    /// </summary>
    /// <param name="fileStream">The stream containing the file data.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="extension">Optional file extension to specify the format.</param>
    /// <param name="config">Optional configuration for the conversion.</param>
    /// <returns>The Markdown result.</returns>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
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

    /// <summary>
    /// Converts a file from a file path to Markdown asynchronously.
    /// Opens the file and delegates to the stream overload.
    /// </summary>
    /// <param name="filePath">The path to the file to convert.</param>
    /// <param name="extension">Optional file extension to specify the format.</param>
    /// <param name="config">Optional configuration for the conversion.</param>
    /// <returns>The Markdown result.</returns>
    public async Task<string> ConvertAsync(string filePath, string? extension = null, MarkDownConfig? config = null)
    {
        _logger.LogInformation("Converting file from path {FilePath}", filePath);
        using var stream = File.OpenRead(filePath);
        string fileName = Path.GetFileName(filePath);
        return await ConvertAsync(stream, fileName, extension, config);
    }

    /// <summary>
    /// Converts a URI to Markdown asynchronously.
    /// Sends a JSON request to the /convert_uri endpoint.
    /// </summary>
    /// <param name="uri">The URI to convert.</param>
    /// <param name="config">Optional configuration for the conversion.</param>
    /// <returns>The Markdown result.</returns>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
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

    /// <summary>
    /// Disposes the HTTP client.
    /// </summary>
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}