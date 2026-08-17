using System.Net;
using Microsoft.Extensions.Options;
using PostForge.Providers.Facebook;

namespace PostForge.Providers.Facebook.Tests;

internal static class FacebookProviderTestFactory
{
    public static (FacebookProvider Provider, FakeHttpMessageHandler Handler) Create(
        Func<HttpRequestMessage, HttpResponseMessage>? responder = null,
        FacebookProviderOptions? options = null)
    {
        var handler = new FakeHttpMessageHandler(responder);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://graph.facebook.com/") };
        var providerOptions = options ?? new FacebookProviderOptions
        {
            AppId = "app-id",
            AppSecret = "app-secret",
            RedirectUri = "https://localhost/callback",
            DefaultPageId = "987654321"
        };

        return (new FacebookProvider(httpClient, Options.Create(providerOptions)), handler);
    }

    public static Dictionary<string, string> ParseQuery(Uri uri)
        => ParseParameters(uri.Query);

    private static Dictionary<string, string> ParseParameters(string? queryString)
    {
        var parameters = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(queryString))
            return parameters;

        foreach (var pair in queryString.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            parameters[WebUtility.UrlDecode(parts[0])] = parts.Length > 1 ? WebUtility.UrlDecode(parts[1]) : string.Empty;
        }

        return parameters;
    }
}