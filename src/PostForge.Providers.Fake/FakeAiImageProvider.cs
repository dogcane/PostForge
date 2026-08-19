using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;

namespace PostForge.Providers.Fake;

public sealed class FakeAiImageProvider : IAiImageProvider
{
    public string ProviderKey => "FAKE";

    public Task<GeneratedImage> GenerateImageAsync(ImageRequest request, CancellationToken ct)
        => Task.FromResult(new GeneratedImage(
            BlobUri: $"https://fake.local/images/{Guid.NewGuid():N}.png",
            ContentType: "image/png"));
}