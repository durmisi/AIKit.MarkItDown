using Aspire.Hosting.Testing;
using AIKit.MarkItDown.Client;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Aspire.Hosting.ApplicationModel;

namespace AIKit.MarkItDown.Tests;

public class E2eTests
{
    private readonly ITestOutputHelper _output;

    public E2eTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task UploadPdfAndVerifyMarkdown()
    {
        // Build and start the distributed application
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AIKit_MarkItDown_AppHost>();
        await using var app = await builder.BuildAsync();
        await app.StartAsync();

        try
        {
            // Wait for the Python API to be ready
            await app.ResourceNotifications.WaitForResourceAsync("MarkItDown", "Running");

            // Create HttpClient with SSL bypass for self-signed certs
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            var endpoint = app.GetEndpoint("MarkItDown", "api");
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(endpoint.AbsoluteUri.ToString()),
                Timeout = TimeSpan.FromMinutes(5)
            };

            // Create logger
            var logger = NullLogger<MarkItDownClient>.Instance;

            // Instantiate the client
            var client = new MarkItDownClient(httpClient, logger);

            // Path to the test PDF
            var pdfPath = Path.Combine(AppContext.BaseDirectory, "pdf-test.pdf");

            // Convert the PDF
            var response = await client.ConvertAsync(pdfPath);

            // Assertions
            Assert.NotNull(response);
            Assert.Equal("pdf-test.pdf", response.Filename);
            Assert.NotNull(response.Markdown);
            Assert.NotEmpty(response.Markdown);
        }
        finally
        {
            await app.StopAsync();
        }
    }
}