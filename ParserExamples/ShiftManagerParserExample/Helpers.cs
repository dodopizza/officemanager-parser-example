using System.Net;
using Microsoft.Extensions.Configuration;

namespace ShiftManagerParserExample;

public static class Helpers
{
    public static Configuration CreateConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.local.json", true)
            .Build();

        return configuration.Get<Configuration>() ?? throw new InvalidOperationException();
    }

    public static HttpClient CreateHttpClient(string? baseUrl, out CookieContainer cookieContainer)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new NullReferenceException($"'{nameof(baseUrl)}' cannot be null or empty");

        cookieContainer = new CookieContainer();

        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            AllowAutoRedirect = true
        };

        return new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute),
            DefaultRequestHeaders =
            {
                { "User-Agent", "dodoextbot" }
            }
        };
    }
}