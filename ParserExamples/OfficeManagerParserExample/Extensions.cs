using AngleSharp.Html.Dom;

namespace OfficeManagerParserExample;

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
}