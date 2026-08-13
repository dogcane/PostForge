import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Post, PostStatus, PostTag, PostTagType } from '../../../models/post.model';

@Component({
  selector: 'app-post-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, MatButtonModule],
  template: `
    <div class="pf-page">
      <div class="pf-page-header">
        <div>
          <h1 class="pf-title">Posts</h1>
          <p class="pf-subtitle">Draft, refine and schedule your content across platforms</p>
        </div>
        <a mat-flat-button class="pf-btn-primary" routerLink="/posts/new">
          <mat-icon>add</mat-icon>
          New Post
        </a>
      </div>

      <ng-container *ngIf="posts.length; else empty">
        <div class="pf-grid pf-grid--posts">
          <article class="pf-card pf-card--hover post-card" *ngFor="let post of posts">
            <div class="post-card__head">
              <span class="pf-status" [class]="statusClass(post.status)">{{ post.status }}</span>
              <span class="post-card__date">{{ post.createdAt | date: 'MMM d' }}</span>
              <button mat-icon-button class="post-card__edit" [routerLink]="['/posts', post.id]" aria-label="Edit post">
                <mat-icon>edit</mat-icon>
              </button>
            </div>
            <p class="post-card__text">{{ post.text }}</p>
            <div class="post-card__platforms">
              <span class="pf-badge" *ngFor="let p of post.targetPlatforms" [class]="'pf-badge--' + p.toLowerCase()">
                <mat-icon>{{ platformIcon(p) }}</mat-icon>
                {{ platformLabel(p) }}
              </span>
            </div>
            <div class="post-card__tags" *ngIf="post.tags.length">
              <span class="pf-tag" *ngFor="let tag of post.tags">
                <mat-icon>{{ tagIcon(tag.tagType) }}</mat-icon>
                @{{ tag.username }} · {{ tagLabel(tag.tagType) }}
              </span>
            </div>
          </article>
        </div>
      </ng-container>

      <ng-template #empty>
        <div class="pf-empty">
          <mat-icon>post_add</mat-icon>
          <h3>No posts yet</h3>
          <p>Create your first post and publish it across your platforms.</p>
          <a mat-flat-button class="pf-btn-primary" routerLink="/posts/new">
            <mat-icon>add</mat-icon>
            Create your first post
          </a>
        </div>
      </ng-template>
    </div>
  `
})
export class PostListComponent {
  posts: Post[] = [
    {
      id: '1',
      text: 'The future of content is not just created — it is forged. Discover how PostForge turns a single idea into a cross-platform launch. #PostForge',
      mediaAssets: [],
      targetPlatforms: ['facebook', 'instagram'],
      tags: [
        { platform: 'facebook', tagType: PostTagType.Collaborator, username: 'silvia.neri' },
        { platform: 'instagram', tagType: PostTagType.Mention, username: 'marco.rossi' }
      ],
      status: PostStatus.Scheduled,
      createdAt: new Date(Date.now() - 86400000 * 2).toISOString(),
      updatedAt: new Date().toISOString()
    },
    {
      id: '2',
      text: 'We just shipped AI caption generation. Describe the vibe, pick the tone, and let the machine do the heavy lifting for your next campaign.',
      mediaAssets: [],
      targetPlatforms: ['youtube', 'tiktok'],
      tags: [],
      status: PostStatus.Published,
      createdAt: new Date(Date.now() - 86400000 * 5).toISOString(),
      updatedAt: new Date().toISOString()
    },
    {
      id: '3',
      text: 'Campaign planning just got a lot easier. Group posts under a goal, set your channels, and keep the whole team aligned on one calendar.',
      mediaAssets: [],
      targetPlatforms: ['facebook'],
      tags: [],
      status: PostStatus.Draft,
      createdAt: new Date(Date.now() - 86400000 * 1).toISOString(),
      updatedAt: new Date().toISOString()
    }
  ];

  statusClass(status: PostStatus): string {
    return 'pf-status--' + status.toLowerCase();
  }

  platformIcon(platform: string): string {
    switch (platform.toLowerCase()) {
      case 'facebook': return 'thumb_up';
      case 'instagram': return 'photo_camera';
      case 'tiktok': return 'music_note';
      case 'youtube': return 'play_circle';
      default: return 'language';
    }
  }

  platformLabel(platform: string): string {
    switch (platform.toLowerCase()) {
      case 'facebook': return 'Facebook';
      case 'instagram': return 'Instagram';
      case 'tiktok': return 'TikTok';
      case 'youtube': return 'YouTube';
      default: return platform;
    }
  }

  tagIcon(tagType: PostTagType): string {
    switch (tagType) {
      case PostTagType.Mention: return 'alternate_email';
      case PostTagType.UserTag: return 'person_pin';
      case PostTagType.Collaborator: return 'group';
      default: return 'person';
    }
  }

  tagLabel(tagType: PostTagType): string {
    switch (tagType) {
      case PostTagType.Mention: return 'Mention';
      case PostTagType.UserTag: return 'Tag on photo';
      case PostTagType.Collaborator: return 'Collaborator';
      default: return tagType;
    }
  }
}
