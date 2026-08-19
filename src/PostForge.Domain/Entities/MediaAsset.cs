using ECO;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class MediaAsset : Entity<Guid>
{
    public Guid Id => Identity;
    public Guid TenantId { get; private set; }
    public string BlobUri { get; private set; }
    public string MediaType { get; private set; }
    public bool GeneratedByAi { get; private set; }
    public string? SourcePrompt { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private MediaAsset() : base(Guid.NewGuid())
    {
        BlobUri = null!;
        MediaType = null!;
    }

    private MediaAsset(Guid tenantId, string blobUri, string mediaType, bool generatedByAi, string? sourcePrompt) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        BlobUri = blobUri;
        MediaType = mediaType;
        GeneratedByAi = generatedByAi;
        SourcePrompt = sourcePrompt;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static OperationResult<MediaAsset> Create(Guid tenantId, string blobUri, string mediaType, bool generatedByAi = false, string? sourcePrompt = null)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(tenantId, "TenantId").Condition(v => v != Guid.Empty)
            .With(blobUri, "BlobUri").Required().StringLength(2048)
            .With(mediaType, "MediaType").Required().StringLength(100);
        if (result.Success && generatedByAi)
            result.With(sourcePrompt, "SourcePrompt").Required().StringLength(1000);
        if (!result.Success)
            return result;
        return OperationResult<MediaAsset>.MakeSuccess(new MediaAsset(tenantId, blobUri, mediaType, generatedByAi, sourcePrompt));
    }
}
