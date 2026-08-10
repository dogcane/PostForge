namespace PostForge.Domain.ValueObjects;

[Flags]
public enum SocialPlatformCapabilities : long
{
    None = 0,

    // Content types
    TextOnly = 1L << 0,
    Photo = 1L << 1,
    Video = 1L << 2,
    ShortVideo = 1L << 3,
    Carousel = 1L << 4,
    Story = 1L << 5,
    Live = 1L << 6,
    Link = 1L << 7,
    Poll = 1L << 8,

    // Content enrichment
    Hashtags = 1L << 9,
    MentionUsers = 1L << 10,
    UserTagWithCoordinates = 1L << 11,
    LocationTag = 1L << 12,
    AltText = 1L << 13,
    CustomThumbnail = 1L << 14,
    Collaborators = 1L << 15,
    PaidPartnership = 1L << 16,
    AiGeneratedLabel = 1L << 17,
    LicensedAudio = 1L << 18,
    CallToAction = 1L << 19,

    // Publishing controls
    NativeScheduling = 1L << 20,
    PrivacyLevels = 1L << 21,
    CommentControls = 1L << 22,
    DuetAndStitchControls = 1L << 23,
    AudienceTargeting = 1L << 24,

    // Post management
    EditPost = 1L << 25,
    DeletePost = 1L << 26,
    ReadUserPosts = 1L << 27,
    PostStatusTracking = 1L << 28,
    MediaUploadApi = 1L << 29,
    Playlists = 1L << 30,

    // Engagement
    ReadComments = 1L << 31,
    ReplyToComments = 1L << 32,
    ModerateComments = 1L << 33,
    ReadMentions = 1L << 34,
    DirectMessaging = 1L << 35,

    // Analytics
    PostInsights = 1L << 36,
    AccountInsights = 1L << 37,
    AudienceInsights = 1L << 38,
}
