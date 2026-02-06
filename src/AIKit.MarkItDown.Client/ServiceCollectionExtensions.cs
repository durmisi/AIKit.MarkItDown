using Microsoft.Extensions.DependencyInjection;

namespace AIKit.MarkItDown.Client;

/// <summary>
/// Extension methods for registering MarkItDown client services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the MarkItDown client to the service collection with the specified base URL.
    /// Registers an HttpClient configured for the MarkItDown API.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="baseUrl">The base URL of the MarkItDown server.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if baseUrl is null or empty.</exception>
    public static IServiceCollection AddMarkItDownClient(this IServiceCollection services, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("Base URL cannot be null or empty.", nameof(baseUrl));
        }

        services.AddHttpClient<MarkItDownClient>((serviceProvider, client) =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }
}