namespace PostForge.Infrastructure.Dtos;

public enum MediaAssetType
{
    Image,
    Video
}

public record MediaUploadDto(
    string BlobUri,
    string FileName,
    string ContentType,
    long ByteSize,
    MediaAssetType Type,
    long? DurationMs = null);
