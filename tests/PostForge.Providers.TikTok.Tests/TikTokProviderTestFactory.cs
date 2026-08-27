using Microsoft.Extensions.Options;

namespace PostForge.Providers.TikTok.Tests;

internal static class TikTokProviderTestFactory
{
    public static (TikTokProvider Provider, FakeHttpMessageHandler Handler) Create(
        Func<HttpRequestMessage, HttpResponseMessage>? responder = null,
        TikTokProviderOptions? options = null)
    {
        var handler = new FakeHttpMessageHandler(responder);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://open.tiktokapis.com/") };
        var providerOptions = options ?? new TikTokProviderOptions
        {
            ClientKey = "client-key",
            ClientSecret = "client-secret",
            RedirectUri = "https://localhost/callback"
        };
        return (new TikTokProvider(httpClient, Options.Create(providerOptions)), handler);
    }
}
