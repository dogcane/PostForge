import { Campaign, CampaignChannel, CampaignGoal } from '../models/campaign.model';
import { Post, PostStatus, PostTagType } from '../models/post.model';
import { ScheduleSlot } from '../models/schedule-slot.model';

export interface DemoUser {
  userId: string;
  email: string;
  isSuperUser: boolean;
  tenantIds: string[];
}

export interface DemoTenant {
  id: string;
  name: string;
  slug: string;
  isActive: boolean;
  createdAtUtc: string;
}

export interface DemoTenantUser {
  tenantId: string;
  userId: string;
  email: string;
  joinedAtUtc: string;
}

export interface TenantPost {
  tenantId: string;
  post: Post;
}

export interface TenantCampaign {
  tenantId: string;
  campaign: Campaign;
}

export interface TenantSlot {
  tenantId: string;
  slot: ScheduleSlot;
}

export interface DemoData {
  users: DemoUser[];
  tenants: DemoTenant[];
  tenantUsers: DemoTenantUser[];
  posts: TenantPost[];
  campaigns: TenantCampaign[];
  slots: TenantSlot[];
}

function iso(daysFromNow: number, hourUtc = 9): string {
  const d = new Date();
  d.setUTCDate(d.getUTCDate() + daysFromNow);
  d.setUTCHours(hourUtc, 0, 0, 0);
  return d.toISOString();
}

export function createDemoData(): DemoData {
  const users: DemoUser[] = [
    { userId: 'u-admin', email: 'admin@postforge.dev', isSuperUser: true, tenantIds: ['t-acme', 't-north'] },
    { userId: 'u-demo', email: 'demo@postforge.dev', isSuperUser: false, tenantIds: ['t-acme'] }
  ];

  const tenants: DemoTenant[] = [
    { id: 't-acme', name: 'Acme Studio', slug: 'acme-studio', isActive: true, createdAtUtc: '2026-01-12T09:00:00Z' },
    { id: 't-north', name: 'Northwind Labs', slug: 'northwind-labs', isActive: true, createdAtUtc: '2026-02-03T09:00:00Z' }
  ];

  const tenantUsers: DemoTenantUser[] = [
    { tenantId: 't-acme', userId: 'u-admin', email: 'admin@postforge.dev', joinedAtUtc: '2026-01-12T09:05:00Z' },
    { tenantId: 't-acme', userId: 'u-demo', email: 'demo@postforge.dev', joinedAtUtc: '2026-01-20T14:30:00Z' },
    { tenantId: 't-acme', userId: 'u-maria', email: 'maria@acme.studio', joinedAtUtc: '2026-02-02T10:15:00Z' },
    { tenantId: 't-north', userId: 'u-admin', email: 'admin@postforge.dev', joinedAtUtc: '2026-02-03T09:05:00Z' }
  ];

  const posts: TenantPost[] = [
    {
      tenantId: 't-acme',
      post: {
        id: 'p-1',
        text: 'Behind the scenes of our studio redesign — new lighting, new backdrop, same chaos. Full tour drops next week!',
        mediaAssets: [],
        targetPlatforms: ['INSTAGRAM', 'FACEBOOK'],
        tags: [],
        status: PostStatus.Draft,
        createdAtUtc: iso(-6),
        updatedAtUtc: iso(-6)
      }
    },
    {
      tenantId: 't-acme',
      post: {
        id: 'p-2',
        text: 'Something big is coming. Teaser drops Friday — can you guess what it is? 👀',
        mediaAssets: [],
        targetPlatforms: ['FACEBOOK', 'INSTAGRAM'],
        tags: [{ platform: 'FACEBOOK', tagType: PostTagType.Mention, username: 'acmestudio' }],
        status: PostStatus.Ready,
        createdAtUtc: iso(-4),
        updatedAtUtc: iso(-3)
      }
    },
    {
      tenantId: 't-acme',
      post: {
        id: 'p-3',
        text: 'Weekly tips #12: three ways to repurpose a single blog post across every platform without sounding repetitive.',
        mediaAssets: [],
        targetPlatforms: ['FACEBOOK'],
        tags: [],
        campaignId: 'c-1',
        status: PostStatus.Scheduled,
        createdAtUtc: iso(-9),
        updatedAtUtc: iso(-9)
      }
    },
    {
      tenantId: 't-acme',
      post: {
        id: 'p-4',
        text: "Launch day recap: we sold out in 4 hours. Thank you! Full restock announcement coming to this space first.",
        mediaAssets: [],
        targetPlatforms: ['FACEBOOK', 'INSTAGRAM', 'TIKTOK'],
        tags: [{ platform: 'INSTAGRAM', tagType: PostTagType.Collaborator, username: 'acme.studio' }],
        campaignId: 'c-1',
        status: PostStatus.Published,
        createdAtUtc: iso(-8),
        updatedAtUtc: iso(-3)
      }
    },
    {
      tenantId: 't-acme',
      post: {
        id: 'p-5',
        text: 'Poll time: which feature should we build next — analytics dashboard or auto-reposting?',
        mediaAssets: [],
        targetPlatforms: ['INSTAGRAM'],
        tags: [],
        campaignId: 'c-2',
        status: PostStatus.Failed,
        createdAtUtc: iso(-5),
        updatedAtUtc: iso(-1)
      }
    },
    {
      tenantId: 't-acme',
      post: {
        id: 'p-6',
        text: 'Customer story: how Bloom & Co grew their newsletter by 40% using our scheduling calendar.',
        mediaAssets: [],
        targetPlatforms: ['YOUTUBE'],
        tags: [],
        status: PostStatus.Scheduled,
        createdAtUtc: iso(-7),
        updatedAtUtc: iso(-7)
      }
    },
    {
      tenantId: 't-north',
      post: {
        id: 'p-7',
        text: 'Roadmap review: Q3 deep dive into the new plugin architecture. Video walkthrough inside.',
        mediaAssets: [],
        targetPlatforms: ['YOUTUBE'],
        tags: [],
        status: PostStatus.Draft,
        createdAtUtc: iso(-2),
        updatedAtUtc: iso(-2)
      }
    },
    {
      tenantId: 't-north',
      post: {
        id: 'p-8',
        text: 'Dev blog digest #7: rate limiting strategies, provider registries, and why we chose source-generated messaging.',
        mediaAssets: [],
        targetPlatforms: ['FACEBOOK'],
        tags: [],
        campaignId: 'c-3',
        status: PostStatus.Ready,
        createdAtUtc: iso(-3),
        updatedAtUtc: iso(-3)
      }
    }
  ];

  const campaigns: TenantCampaign[] = [
    {
      tenantId: 't-acme',
      campaign: {
        id: 'c-1',
        name: 'Spring Launch',
        goal: CampaignGoal.Awareness,
        channel: CampaignChannel.Organic,
        startDateUtc: iso(-10),
        endDateUtc: iso(20),
        postIds: ['p-3', 'p-4'],
        createdAtUtc: iso(-10)
      }
    },
    {
      tenantId: 't-acme',
      campaign: {
        id: 'c-2',
        name: 'Feature Poll Push',
        goal: CampaignGoal.LeadGeneration,
        channel: CampaignChannel.Paid,
        startDateUtc: iso(-5),
        endDateUtc: iso(9),
        postIds: ['p-5'],
        createdAtUtc: iso(-5)
      }
    },
    {
      tenantId: 't-north',
      campaign: {
        id: 'c-3',
        name: 'Dev Blog Digest',
        goal: CampaignGoal.Reputation,
        channel: CampaignChannel.Organic,
        startDateUtc: iso(-3),
        endDateUtc: iso(27),
        postIds: ['p-8'],
        createdAtUtc: iso(-3)
      }
    }
  ];

  const slots: TenantSlot[] = [
    { tenantId: 't-acme', slot: { id: 's-1', postId: 'p-3', platform: 'FACEBOOK', scheduledAtUtc: iso(2, 9), status: PostStatus.Scheduled, retryCount: 0 } },
    { tenantId: 't-acme', slot: { id: 's-2', postId: 'p-6', platform: 'YOUTUBE', scheduledAtUtc: iso(5, 15), status: PostStatus.Scheduled, retryCount: 0 } },
    { tenantId: 't-acme', slot: { id: 's-3', postId: 'p-4', platform: 'INSTAGRAM', scheduledAtUtc: iso(-3, 11), status: PostStatus.Published, retryCount: 0, publishedAtUtc: iso(-3, 11) } },
    { tenantId: 't-acme', slot: { id: 's-4', postId: 'p-4', platform: 'FACEBOOK', scheduledAtUtc: iso(-3, 11), status: PostStatus.Published, retryCount: 0, publishedAtUtc: iso(-3, 11) } },
    { tenantId: 't-acme', slot: { id: 's-5', postId: 'p-5', platform: 'INSTAGRAM', scheduledAtUtc: iso(-1, 18), status: PostStatus.Failed, retryCount: 2, lastError: 'Instagram API rate limit exceeded (code 4). Next retry in 15 minutes.' } },
    { tenantId: 't-north', slot: { id: 's-6', postId: 'p-8', platform: 'FACEBOOK', scheduledAtUtc: iso(3, 10), status: PostStatus.Scheduled, retryCount: 0 } }
  ];

  return { users, tenants, tenantUsers, posts, campaigns, slots };
}
