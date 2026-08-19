import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import {
  Post,
  postStatusClass,
  postStatusLabel,
  postTagIcon,
  postTagTypeLabel
} from '../../../models/post.model';
import { platformBadgeClass, platformIcon, platformLabel } from '../../../models/platform.model';
import { ApiService } from '../../../services/api.service';

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

      <div class="pf-alert" *ngIf="error">{{ error }}</div>

      <ng-container *ngIf="!loading; else loadingState">
        <ng-container *ngIf="posts.length; else empty">
          <div class="pf-grid pf-grid--posts">
            <article class="pf-card pf-card--hover post-card" *ngFor="let post of posts">
              <div class="post-card__head">
                <span class="pf-status" [class]="postStatusClass(post.status)">{{ postStatusLabel(post.status) }}</span>
                <span class="post-card__date">{{ post.createdAtUtc | date: 'MMM d' }}</span>
                <button mat-icon-button class="post-card__edit" [routerLink]="['/posts', post.id]" aria-label="Edit post">
                  <mat-icon>edit</mat-icon>
                </button>
              </div>
              <p class="post-card__text">{{ post.text }}</p>
              <div class="post-card__platforms">
                <span class="pf-badge" *ngFor="let p of post.targetPlatforms" [class]="platformBadgeClass(p)">
                  <mat-icon>{{ platformIcon(p) }}</mat-icon>
                  {{ platformLabel(p) }}
                </span>
              </div>
              <div class="post-card__tags" *ngIf="post.tags.length">
                <span class="pf-tag" *ngFor="let tag of post.tags">
                  <mat-icon>{{ postTagIcon(tag.tagType) }}</mat-icon>
                  @{{ tag.username }} · {{ postTagTypeLabel(tag.tagType) }}
                </span>
              </div>
              <div class="post-card__foot">
                <a mat-stroked-button class="pf-btn-schedule" [routerLink]="['/scheduling']">
                  <mat-icon>schedule</mat-icon>
                  Schedule
                </a>
                <button mat-icon-button class="post-card__delete" (click)="deletePost(post)" aria-label="Delete post">
                  <mat-icon>delete_outline</mat-icon>
                </button>
              </div>
            </article>
          </div>
        </ng-container>
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

      <ng-template #loadingState>
        <div class="pf-loading"><mat-icon>autorenew</mat-icon> Loading posts...</div>
      </ng-template>
    </div>
  `
})
export class PostListComponent implements OnInit {
  posts: Post[] = [];
  loading = false;
  error: string | null = null;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading = true;
    this.error = null;
    this.api.getPosts().subscribe({
      next: (posts) => {
        this.posts = posts;
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load posts.';
        this.loading = false;
      }
    });
  }

  deletePost(post: Post): void {
    if (!confirm(`Delete this post? This cannot be undone.`)) {
      return;
    }
    this.api.deletePost(post.id).subscribe({
      next: () => this.load(),
      error: () => {
        this.error = 'Unable to delete the post.';
      }
    });
  }

  readonly postStatusClass = postStatusClass;
  readonly postStatusLabel = postStatusLabel;
  readonly postTagIcon = postTagIcon;
  readonly postTagTypeLabel = postTagTypeLabel;
  readonly platformBadgeClass = platformBadgeClass;
  readonly platformIcon = platformIcon;
  readonly platformLabel = platformLabel;
}