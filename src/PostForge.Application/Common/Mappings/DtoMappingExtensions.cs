using PostForge.Application.Campaigns.DTOs;
using PostForge.Application.Posts.DTOs;
using PostForge.Application.ProviderCredentials.DTOs;
using PostForge.Application.Scheduling.DTOs;
using PostForge.Application.Tenants.DTOs;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Common.Mappings;

public static class DtoMappingExtensions
{
    public static PostDto ToDto(this Post post) => new()
    {
        Id = post.Id,
        Text = post.Text,
        MediaAssets = post.MediaAssets.ToList(),
        TargetPlatforms = post.TargetPlatforms.ToList(),
        Tags = post.Tags.Select(t => t.ToDto()).ToList(),
        CampaignId = post.CampaignId,
        Status = post.Status,
        CreatedAtUtc = post.CreatedAtUtc,
        UpdatedAtUtc = post.UpdatedAtUtc
    };

    public static PostTagDto ToDto(this PostTag tag) => new(tag.Platform, tag.TagType, tag.Username);

    public static CampaignDto ToDto(this Campaign campaign) => new()
    {
        Id = campaign.Id,
        Name = campaign.Name,
        Goal = campaign.Goal,
        Channel = campaign.Channel,
        StartDateUtc = campaign.StartDateUtc,
        EndDateUtc = campaign.EndDateUtc,
        PostIds = campaign.PostIds.ToList(),
        CreatedAtUtc = campaign.CreatedAtUtc
    };

    public static ScheduleSlotDto ToDto(this ScheduleSlot slot) => new()
    {
        Id = slot.Id,
        PostId = slot.PostId,
        Platform = slot.Platform,
        ScheduledAtUtc = slot.ScheduledAtUtc,
        Status = slot.Status,
        RetryCount = slot.RetryCount,
        LastError = slot.LastError,
        PublishedAtUtc = slot.PublishedAtUtc
    };

    public static TenantDto ToDto(this Tenant tenant) => new()
    {
        Id = tenant.Id,
        Name = tenant.Name,
        Slug = tenant.Slug,
        IsActive = tenant.IsActive,
        CreatedAtUtc = tenant.CreatedAtUtc
    };

    public static ProviderCredentialDto ToDto(this ProviderCredential credential) => new()
    {
        Id = credential.Id,
        ProviderKey = credential.ProviderKey,
        Scope = credential.Scope,
        DisplayName = credential.DisplayName,
        Description = credential.Description,
        KeyVaultReference = credential.KeyVaultReference,
        MaskedSecret = credential.GetMaskedSecret(),
        HasSecret = !string.IsNullOrWhiteSpace(credential.SecretValue),
        SettingsJson = credential.SettingsJson,
        IsEnabled = credential.IsEnabled,
        IsValidated = credential.IsValidated,
        CreatedAtUtc = credential.CreatedAtUtc,
        UpdatedAtUtc = credential.UpdatedAtUtc
    };
}