using System.Text;
using AngleSharp.Html.Dom;

namespace AdminWebParserExample;

public static class Extensions
{
    public static string? GetInputValue(this IHtmlDocument document, string name)
    {
        return document.QuerySelector($"input[name=\"{name}\"]")?.Attributes["value"]?.Value;
    }

    public static async Task<HttpResponseMessage> PostAsFormUrlEncodedAsync(this HttpClient httpClient,
        string path,
        IReadOnlyDictionary<string, string?> formData)
    {
        var content = new FormUrlEncodedContent(formData);

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(path, UriKind.Relative),
            Content = content
        };

        var response = await httpClient.SendAsync(request);
        return response;
    }

    public static async Task<HttpResponseMessage> PostAsJsonAsync(this HttpClient httpClient,
        string path,
        string jsonContent)
    {
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(path, UriKind.Relative),
            Content = content
        };

        var response = await httpClient.SendAsync(request);
        return response;
    }
}