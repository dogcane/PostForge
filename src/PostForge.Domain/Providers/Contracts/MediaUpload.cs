namespace PostForge.Domain.Providers.Contracts;

public enum MediaAssetType
{
    Image,
    Video
}

public record MediaUpload(
    string BlobUri,
    string FileName,
    string ContentType,
    long ByteSize,
    MediaAssetType Type,
    long? DurationMs = null);