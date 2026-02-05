using Microsoft.Extensions.DependencyInjection;

namespace AIKit.MarkItDown.Client;

public static class ServiceCollectionExtensions
{
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