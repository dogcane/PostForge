using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;

namespace PostForge.Infrastructure.DAL;

public class PostForgeDbContext : DbContext
{
    public Guid? CurrentTenantId { get; set; }

    public PostForgeDbContext(DbContextOptions<PostForgeDbContext> options) : base(options)
    {
    }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<ScheduleSlot> ScheduleSlots => Set<ScheduleSlot>();
    public DbSet<SocialAccount> SocialAccounts => Set<SocialAccount>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<ProviderCredential> ProviderCredentials => Set<ProviderCredential>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var postStatusConverter = new ValueConverter<PostStatus, string>(
            v => v.ToString(),
            v => Enum.Parse<PostStatus>(v));

        var campaignGoalConverter = new ValueConverter<CampaignGoal, string>(
            v => v.ToString(),
            v => Enum.Parse<CampaignGoal>(v));

        var campaignChannelConverter = new ValueConverter<CampaignChannel, string>(
            v => v.ToString(),
            v => Enum.Parse<CampaignChannel>(v));

        var providerCredentialScopeConverter = new ValueConverter<ProviderCredentialScope, string>(
            v => v.ToString(),
            v => Enum.Parse<ProviderCredentialScope>(v));

        var postTagTypeConverter = new ValueConverter<PostTagType, string>(
            v => v.ToString(),
            v => Enum.Parse<PostTagType>(v));

        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("Posts");
            entity.HasKey(e => e.Identity);
            entity.Property(e => e.Identity).HasColumnName("Id");
            entity.Property(e => e.TenantId);
            entity.HasIndex(e => e.TenantId);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(5000);
            entity.Property(e => e.Status).HasConversion(postStatusConverter).HasMaxLength(50);
            entity.Property(e => e.CampaignId);
            entity.Property(e => e.CreatedAtUtc);
            entity.Property(e => e.UpdatedAtUtc);
            entity.Ignore(e => e.TargetPlatforms);
            entity.HasQueryFilter(p => CurrentTenantId == null || p.TenantId == CurrentTenantId);

            entity.OwnsMany(e => e.Tags, tag =>
            {
                tag.ToTable("PostTags");
                tag.WithOwner().HasForeignKey("PostId");
                tag.Property<int>("Id");
                tag.HasKey("Id");
                tag.Property(t => t.Platform).IsRequired().HasMaxLength(50);
                tag.Property(t => t.TagType).HasConversion(postTagTypeConverter).HasMaxLength(50);
                tag.Property(t => t.Username).IsRequired().HasMaxLength(200);
            });

            entity.HasMany<MediaAsset>("_mediaAssetsField")
                .WithOne()
                .HasForeignKey("PostId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation("_mediaAssetsField").AutoInclude();
        });

        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.ToTable("MediaAssets");
            entity.HasKey(e => e.Identity);
            entity.Property(e => e.Identity).HasColumnName("Id");
            entity.Property(e => e.TenantId);
            entity.HasIndex(e => e.TenantId);
            entity.Property(e => e.BlobUri).IsRequired().HasMaxLength(2048);
            entity.Property(e => e.MediaType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.GeneratedByAi);
            entity.Property(e => e.SourcePrompt).HasMaxLength(1000);
            entity.Property(e => e.CreatedAtUtc);
            entity.HasQueryFilter(m => CurrentTenantId == null || m.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.ToTable("Campaigns");
            entity.HasKey(e => e.Identity);
            entity.Property(e => e.Identity).HasColumnName("Id");
            entity.Property(e => e.TenantId);
            entity.HasIndex(e => e.TenantId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Goal).HasConversion(campaignGoalConverter).HasMaxLength(50);
            entity.Property(e => e.Channel).HasConversion(campaignChannelConverter).HasMaxLength(50);
            entity.Property(e => e.StartDateUtc);
            entity.Property(e => e.EndDateUtc);
            entity.Property(e => e.CreatedAtUtc);
            entity.Ignore(e => e.PostIds);
            entity.HasQueryFilter(c => CurrentTenantId == null || c.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<ScheduleSlot>(entity =>
        {
            entity.ToTable("ScheduleSlots");
            entity.HasKey(e => e.Identity);
            entity.Property(e => e.Identity).HasColumnName("Id");
            entity.Property(e => e.TenantId);
            entity.HasIndex(e => e.TenantId);
            entity.Property(e => e.PostId);
            entity.Property(e => e.Platform).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ScheduledAtUtc);
            entity.Property(e => e.Status).HasConversion(postStatusConverter).HasMaxLength(50);
            entity.Property(e => e.RetryCount);
            entity.Property(e => e.LastError).HasMaxLength(2000);
            entity.Property(e => e.PublishedAtUtc);
            entity.HasQueryFilter(s => CurrentTenantId == null || s.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<SocialAccount>(entity =>
        {
            entity.ToTable("SocialAccounts");
            entity.HasKey(e => e.Identity);
            entity.Property(e => e.Identity).HasColumnName("Id");
            entity.Property(e => e.TenantId);
            entity.HasIndex(e => e.TenantId);
            entity.Property(e => e.Platform).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.OAuthTokens).IsRequired();
            entity.Property(e => e.CreatedAtUtc);
            entity.Property(e => e.LastRefreshedAtUtc);
            entity.HasQueryFilter(s => CurrentTenantId == null || s.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<ProviderCredential>(entity =>
        {
            entity.ToTable("ProviderCredentials");
            entity.HasKey(e => e.Identity);
            entity.Property(e => e.Identity).HasColumnName("Id");
            entity.Property(e => e.TenantId);
            entity.HasIndex(e => e.TenantId);
            entity.Property(e => e.ProviderKey).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Scope).HasConversion(providerCredentialScopeConverter).HasMaxLength(50);
            entity.Property(e => e.KeyVaultReference).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IsValidated);
            entity.Property(e => e.CreatedAtUtc);
            entity.HasQueryFilter(c => CurrentTenantId == null || c.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(e => e.Identity);
            entity.Property(e => e.Identity).HasColumnName("Id");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.CreatedAtUtc);
            entity.Property(e => e.IsActive);
        });

        modelBuilder.Entity<TenantMembership>(entity =>
        {
            entity.ToTable("TenantMemberships");
            entity.HasKey(e => e.Identity);
            entity.Property(e => e.Identity).HasColumnName("Id");
            entity.Property(e => e.TenantId);
            entity.Property(e => e.UserId);
            entity.Property(e => e.JoinedAtUtc);
            entity.HasIndex(e => new { e.TenantId, e.UserId }).IsUnique();
            entity.HasQueryFilter(m => CurrentTenantId == null || m.TenantId == CurrentTenantId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
