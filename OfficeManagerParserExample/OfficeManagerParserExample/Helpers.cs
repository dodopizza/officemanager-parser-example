using System.Net;

namespace OfficeManagerParserExample;

public static class Helpers
{
    public static HttpClient CreateHttpClient(string baseUrl, out CookieContainer cookieContainer)
    {
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