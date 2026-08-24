import {
  HttpErrorResponse,
  HttpEvent,
  HttpInterceptorFn,
  HttpRequest,
  HttpResponse
} from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { delay } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Campaign, CampaignChannel, CampaignGoal, CreateCampaignRequest, UpdateCampaignRequest } from '../models/campaign.model';
import { CaptionRequest, CaptionResult, ImageRequest, ImageResult } from '../models/ai.model';
import { ChangePostStatusRequest, CreatePostRequest, Post, PostStatus, UpdatePostRequest } from '../models/post.model';
import { MarkSlotFailedRequest, ScheduleRequest, ScheduleSlot, ScheduleSlotStatus } from '../models/schedule-slot.model';
import { AddTenantUserRequest, CreateTenantRequest, Tenant, TenantUser } from '../models/tenant.model';
import { CurrentUser, LoginResult } from '../models/user.model';
import { createDemoData, DemoData, DemoUser } from './demo-data';

const DEMO_LATENCY_MS = 250;
const TOKEN_PREFIX = 'demo-token-';

export const demoInterceptor: HttpInterceptorFn = (req, next) => {
  if (!environment.demoMode || !req.url.startsWith('/api/')) {
    return next(req);
  }
  const backend = inject(DemoBackend);
  return backend.handle(req);
};

@Injectable({ providedIn: 'root' })
export class DemoBackend {
  private readonly data: DemoData = createDemoData();

  handle(req: HttpRequest<unknown>): Observable<HttpEvent<unknown>> {
    try {
      return of(this.route(req)).pipe(delay(DEMO_LATENCY_MS));
    } catch (error) {
      const err =
        error instanceof HttpErrorResponse
          ? error
          : new HttpErrorResponse({ status: 500, statusText: 'Demo backend error', url: req.url });
      return throwError(() => err).pipe(delay(DEMO_LATENCY_MS));
    }
  }

  private route(req: HttpRequest<unknown>): HttpResponse<unknown> {
    const path = req.url.split('?')[0];
    const method = req.method.toUpperCase();
    const body = () => req.body as Record<string, unknown>;

    // ---- Auth ----
    if (method === 'POST' && path === '/api/v1/auth/login') {
      const email = String(body()['email'] ?? '').trim().toLowerCase();
      const user = this.data.users.find((u) => u.email.toLowerCase() === email);
      if (!user) {
        throw unauthorized('Invalid email or password.');
      }
      const result: LoginResult = {
        token: TOKEN_PREFIX + user.userId,
        expiresAtUtc: new Date(Date.now() + 8 * 3600_000).toISOString(),
        userId: user.userId,
        email: user.email,
        isSuperUser: user.isSuperUser
      };
      return ok(result);
    }

    if (method === 'GET' && path === '/api/v1/auth/me') {
      const user = this.userFromToken(req);
      return ok(this.toCurrentUser(user));
    }

    // ---- Tenants ----
    if (method === 'GET' && path === '/api/v1/tenants') {
      const user = this.userFromToken(req);
      const visible = user.isSuperUser ? this.data.tenants : this.data.tenants.filter((t) => user.tenantIds.includes(t.id));
      return ok(visible);
    }

    if (method === 'POST' && path === '/api/v1/tenants') {
      const request = body() as unknown as CreateTenantRequest;
      const user = this.userFromToken(req);
      const tenant: Tenant = {
        id: newId(),
        name: request.name,
        slug: request.slug,
        isActive: true,
        createdAtUtc: new Date().toISOString()
      };
      this.data.tenants.push(tenant);
      user.tenantIds.push(tenant.id);
      this.data.tenantUsers.push({ tenantId: tenant.id, userId: user.userId, email: user.email, joinedAtUtc: new Date().toISOString() });
      return ok(tenant.id);
    }

    let m = path.match(/^\/api\/v1\/tenants\/([^/]+)\/users$/);
    if (m) {
      const tenantId = m[1];
      this.requireTenant(tenantId);
      if (method === 'GET') {
        const members: TenantUser[] = this.data.tenantUsers
          .filter((x) => x.tenantId === tenantId)
          .map((x) => ({ userId: x.userId, email: x.email, joinedAtUtc: x.joinedAtUtc }));
        return ok(members);
      }
      if (method === 'POST') {
        const request = body() as unknown as AddTenantUserRequest;
        const existing = this.data.tenantUsers.find((x) => x.tenantId === tenantId && x.email === request.email);
        if (existing) {
          return ok(existing.userId);
        }
        const userId = newId();
        this.data.tenantUsers.push({ tenantId, userId, email: request.email, joinedAtUtc: new Date().toISOString() });
        return ok(userId);
      }
    }

    m = path.match(/^\/api\/v1\/tenants\/([^/]+)\/users\/([^/]+)$/);
    if (m && method === 'DELETE') {
      const [, tenantId, userId] = m;
      this.requireTenant(tenantId);
      this.data.tenantUsers = this.data.tenantUsers.filter((x) => !(x.tenantId === tenantId && x.userId === userId));
      return ok(null);
    }

    m = path.match(/^\/api\/v1\/tenants\/([^/]+)$/);
    if (m && method === 'GET') {
      return ok(this.requireTenant(m[1]));
    }

    // ---- Posts ----
    if (method === 'GET' && path === '/api/v1/posts') {
      return ok(this.postsForTenant(req).map((p) => p.post));
    }

    if (method === 'POST' && path === '/api/v1/posts') {
      const request = body() as unknown as CreatePostRequest;
      const now = new Date().toISOString();
      const post: Post = {
        id: newId(),
        text: request.text,
        mediaAssets: [],
        targetPlatforms: [...(request.targetPlatforms ?? [])],
        tags: [...(request.tags ?? [])],
        campaignId: request.campaignId,
        status: PostStatus.Draft,
        createdAtUtc: now,
        updatedAtUtc: now
      };
      const targetTenantId = this.tenantIdOf(req) ?? this.resolveDefaultTenant(req);
      this.data.posts.push({ tenantId: targetTenantId, post });
      return ok(post.id);
    }

    m = path.match(/^\/api\/v1\/posts\/([^/]+)\/status$/);
    if (m && method === 'PATCH') {
      const request = body() as unknown as ChangePostStatusRequest;
      const found = this.requirePost(request.postId);
      found.post.status = request.newStatus;
      found.post.updatedAtUtc = new Date().toISOString();
      return ok(null);
    }

    m = path.match(/^\/api\/v1\/posts\/([^/]+)$/);
    if (m) {
      const found = this.requirePost(m[1]);
      if (method === 'GET') {
        return ok(found.post);
      }
      if (method === 'PUT') {
        const request = body() as unknown as UpdatePostRequest;
        found.post.text = request.text;
        if (request.mediaAssets !== undefined) {
          found.post.mediaAssets = [...request.mediaAssets];
        }
        if (request.targetPlatforms !== undefined) {
          found.post.targetPlatforms = [...request.targetPlatforms];
        }
        if (request.tags !== undefined) {
          found.post.tags = [...request.tags];
        }
        found.post.updatedAtUtc = new Date().toISOString();
        return ok(null);
      }
      if (method === 'DELETE') {
        this.data.posts = this.data.posts.filter((p) => p.post.id !== found.post.id);
        this.data.slots = this.data.slots.filter((s) => s.slot.postId !== found.post.id);
        return ok(null);
      }
    }

    // ---- Campaigns ----
    if (method === 'GET' && path === '/api/v1/campaigns') {
      return ok(this.campaignsForTenant(req).map((c) => c.campaign));
    }

    if (method === 'POST' && path === '/api/v1/campaigns') {
      const request = body() as unknown as CreateCampaignRequest;
      const campaign: Campaign = {
        id: newId(),
        name: request.name,
        goal: request.goal,
        channel: request.channel,
        startDateUtc: request.startDateUtc,
        endDateUtc: request.endDateUtc,
        postIds: [],
        createdAtUtc: new Date().toISOString()
      };
      const targetTenantId = this.tenantIdOf(req) ?? this.resolveDefaultTenant(req);
      this.data.campaigns.push({ tenantId: targetTenantId, campaign });
      return ok(campaign.id);
    }

    m = path.match(/^\/api\/v1\/campaigns\/([^/]+)$/);
    if (m) {
      const found = this.data.campaigns.find((c) => c.campaign.id === m![1]);
      if (!found) {
        throw notFound('Campaign not found.');
      }
      if (method === 'GET') {
        return ok(found.campaign);
      }
      if (method === 'PUT') {
        const request = body() as unknown as UpdateCampaignRequest;
        found.campaign.name = request.name;
        found.campaign.goal = request.goal;
        found.campaign.channel = request.channel;
        found.campaign.startDateUtc = request.startDateUtc;
        found.campaign.endDateUtc = request.endDateUtc;
        return ok(null);
      }
      if (method === 'DELETE') {
        this.data.campaigns = this.data.campaigns.filter((c) => c.campaign.id !== found.campaign.id);
        for (const entry of this.data.posts) {
          if (entry.post.campaignId === found.campaign.id) {
            entry.post.campaignId = undefined;
          }
        }
        return ok(null);
      }
    }

    // ---- Scheduling ----
    if (method === 'POST' && path === '/api/v1/scheduling/schedule') {
      const request = body() as unknown as ScheduleRequest;
      const found = this.requirePost(request.postId);
      const slot: ScheduleSlot = {
        id: newId(),
        postId: request.postId,
        platform: request.platform,
        scheduledAtUtc: request.scheduledAtUtc,
        status: ScheduleSlotStatus.Scheduled,
        retryCount: 0
      };
      this.data.slots.push({ tenantId: found.tenantId, slot });
      if (found.post.status === PostStatus.Draft || found.post.status === PostStatus.Ready) {
        found.post.status = PostStatus.Scheduled;
        found.post.updatedAtUtc = new Date().toISOString();
      }
      return ok(slot.id);
    }

    if (method === 'GET' && path === '/api/v1/scheduling/pending') {
      const tenantId = this.tenantIdOf(req);
      return ok(
        this.data.slots
          .filter((s) => (tenantId === null || s.tenantId === tenantId) && (s.slot.status === ScheduleSlotStatus.Scheduled || s.slot.status === ScheduleSlotStatus.Publishing))
          .map((s) => s.slot)
      );
    }

    if (method === 'GET' && path === '/api/v1/scheduling/calendar') {
      const tenantId = this.tenantIdOf(req);
      const start = new Date(String(req.params.get('start')));
      const end = new Date(String(req.params.get('end')));
      return ok(
        this.data.slots
          .filter((s) => {
            if (tenantId !== null && s.tenantId !== tenantId) {
              return false;
            }
            const at = new Date(s.slot.scheduledAtUtc);
            return at >= start && at < end;
          })
          .map((s) => s.slot)
      );
    }

    m = path.match(/^\/api\/v1\/scheduling\/by-post\/([^/]+)$/);
    if (m && method === 'GET') {
      const tenantId = this.tenantIdOf(req);
      return ok(this.data.slots.filter((s) => (tenantId === null || s.tenantId === tenantId) && s.slot.postId === m![1]).map((s) => s.slot));
    }

    m = path.match(/^\/api\/v1\/scheduling\/([^/]+)\/publish$/);
    if (m && method === 'POST') {
      const entry = this.requireSlot(m[1]);
      entry.slot.status = ScheduleSlotStatus.Published;
      entry.slot.publishedAtUtc = new Date().toISOString();
      const postEntry = this.data.posts.find((p) => p.post.id === entry.slot.postId);
      if (postEntry) {
        postEntry.post.status = PostStatus.Published;
        postEntry.post.updatedAtUtc = entry.slot.publishedAtUtc!;
      }
      return ok(null);
    }

    m = path.match(/^\/api\/v1\/scheduling\/([^/]+)\/fail$/);
    if (m && method === 'POST') {
      const request = body() as unknown as MarkSlotFailedRequest;
      const entry = this.requireSlot(request.slotId);
      entry.slot.status = ScheduleSlotStatus.Failed;
      entry.slot.lastError = request.error;
      entry.slot.retryCount += 1;
      const postEntry = this.data.posts.find((p) => p.post.id === entry.slot.postId);
      if (postEntry) {
        postEntry.post.status = PostStatus.Failed;
        postEntry.post.updatedAtUtc = new Date().toISOString();
      }
      return ok(null);
    }

    // ---- AI ----
    if (method === 'POST' && path === '/api/v1/ai/caption') {
      return ok(this.generateCaption(body() as unknown as CaptionRequest));
    }

    if (method === 'POST' && path === '/api/v1/ai/image') {
      return ok(this.generateImage(body() as unknown as ImageRequest));
    }

    throw notFound(`No demo handler for ${req.method} ${path}`);
  }

  // ---- Helpers ----

  private userFromToken(req: HttpRequest<unknown>): DemoUser {
    const token = req.headers.get('Authorization')?.replace(/^Bearer\s+/i, '') ?? '';
    const userId = token.startsWith(TOKEN_PREFIX) ? token.slice(TOKEN_PREFIX.length) : null;
    const user = this.data.users.find((u) => u.userId === userId);
    if (!user) {
      throw unauthorized('Session expired.');
    }
    return user;
  }

  private toCurrentUser(user: DemoUser): CurrentUser {
    return {
      userId: user.userId,
      email: user.email,
      isSuperUser: user.isSuperUser,
      tenants: user.tenantIds
        .map((id) => this.data.tenants.find((t) => t.id === id))
        .filter((t): t is NonNullable<typeof t> => !!t)
    };
  }

  private tenantIdOf(req: HttpRequest<unknown>): string | null {
    return req.headers.get('X-Tenant-Id');
  }

  private resolveDefaultTenant(req: HttpRequest<unknown>): string {
    try {
      const user = this.userFromToken(req);
      return user.tenantIds[0] ?? 't-acme';
    } catch {
      return 't-acme';
    }
  }

  private requireTenant(id: string): Tenant {
    const tenant = this.data.tenants.find((t) => t.id === id);
    if (!tenant) {
      throw notFound('Tenant not found.');
    }
    return tenant;
  }

  private postsForTenant(req: HttpRequest<unknown>): DemoData['posts'] {
    const tenantId = this.tenantIdOf(req);
    if (tenantId === null) {
      return this.data.posts;
    }
    return this.data.posts.filter((p) => p.tenantId === tenantId);
  }

  private campaignsForTenant(req: HttpRequest<unknown>): DemoData['campaigns'] {
    const tenantId = this.tenantIdOf(req);
    if (tenantId === null) {
      return this.data.campaigns;
    }
    return this.data.campaigns.filter((c) => c.tenantId === tenantId);
  }

  private requirePost(id: string): DemoData['posts'][number] {
    const found = this.data.posts.find((p) => p.post.id === id);
    if (!found) {
      throw notFound('Post not found.');
    }
    return found;
  }

  private requireSlot(id: string): DemoData['slots'][number] {
    const found = this.data.slots.find((s) => s.slot.id === id);
    if (!found) {
      throw notFound('Schedule slot not found.');
    }
    return found;
  }

  private generateCaption(request: CaptionRequest): CaptionResult {
    const brief = (request.brief ?? '').trim() || 'our latest update';
    const tone = (request.tone ?? '').toLowerCase();
    const platform = (request.platform ?? '').toLowerCase();

    const openers: Record<string, string> = {
      professional: "Here's what matters this week:",
      playful: 'Guess what just happened?',
      bold: "Let's cut to the chase:",
      friendly: 'Quick update for you:'
    };

    const hashtags: Record<string, string> = {
      instagram: '\n\n#instadaily #behindthescenes #contentcreation',
      tiktok: '\n\n#fyp #contentcreator #smallbusiness',
      facebook: '',
      youtube: '\n\nWatch the full breakdown on our channel.'
    };

    const opener = openers[tone] ?? openers['friendly'];
    return {
      caption:
        `${opener} ${brief}.\n\n` +
        `We put together something we think you'll love — swipe through, tell us what you think, ` +
        `and stay tuned for what's coming next.` +
        (hashtags[platform] ?? '')
    };
  }

  private generateImage(request: ImageRequest): ImageResult {
    const prompt = (request.prompt ?? '').replace(/[<>&"]/g, '').slice(0, 60);
    const svg =
      `<svg xmlns="http://www.w3.org/2000/svg" width="800" height="500">` +
      `<rect width="800" height="500" fill="#eef0f3"/>` +
      `<rect width="800" height="6" fill="#6c5cf6"/>` +
      `<rect x="330" y="150" width="140" height="140" rx="16" fill="#6c5cf6"/>` +
      `<text x="400" y="232" font-family="sans-serif" font-size="64" fill="#ffffff" text-anchor="middle">AI</text>` +
      `<text x="400" y="350" font-family="sans-serif" font-size="24" font-weight="600" fill="#191b23" text-anchor="middle">Demo preview</text>` +
      `<text x="400" y="386" font-family="sans-serif" font-size="15" fill="#5c6070" text-anchor="middle">${prompt}</text>` +
      `</svg>`;
    return {
      blobUri: 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg),
      prompt: request.prompt
    };
  }
}

function ok<T>(body: T): HttpResponse<T> {
  return new HttpResponse({ status: 200, body });
}

function notFound(message: string): HttpErrorResponse {
  return new HttpErrorResponse({ status: 404, statusText: message });
}

function unauthorized(message: string): HttpErrorResponse {
  return new HttpErrorResponse({ status: 401, statusText: message });
}

function newId(): string {
  return typeof crypto !== 'undefined' && 'randomUUID' in crypto
    ? crypto.randomUUID()
    : `${Date.now()}-${Math.random().toString(36).slice(2, 10)}`;
}
