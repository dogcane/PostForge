using Microsoft.Extensions.Options;

namespace PostForge.Providers.YouTube.Tests;

internal static class YouTubeProviderTestFactory
{
    public static (YouTubeProvider Provider, FakeHttpMessageHandler Handler) Create(
        Func<HttpRequestMessage, HttpResponseMessage>? responder = null,
        YouTubeProviderOptions? options = null)
    {
        var handler = new FakeHttpMessageHandler(responder);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/") };
        var providerOptions = options ?? new YouTubeProviderOptions
        {
            ClientId = "client-id",
            ClientSecret = "client-secret",
            RedirectUri = "https://localhost/callback"
        };
        return (new YouTubeProvider(httpClient, Options.Create(providerOptions)), handler);
    }
}
