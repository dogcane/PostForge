import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { finalize, forkJoin, map, switchMap } from 'rxjs';
import {
  CreatePostRequest,
  Post,
  PostTag,
  PostTagType,
  UpdatePostRequest
} from '../../../models/post.model';
import { PLATFORM_OPTIONS } from '../../../models/platform.model';
import { ApiService } from '../../../services/api.service';

interface TagTypeOption {
  value: PostTagType;
  label: string;
}

@Component({
  selector: 'app-post-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="pf-page pf-form">
      <div class="pf-page-header">
        <div>
          <h1 class="pf-title">{{ isEditing ? 'Edit Post' : 'New Post' }}</h1>
          <p class="pf-subtitle">Write once, publish everywhere.</p>
        </div>
      </div>

      <div class="pf-alert" *ngIf="error">{{ error }}</div>

      <mat-card class="pf-card pf-form__card">
        <div class="pf-form__title">
          <div class="pf-feature-icon">
            <mat-icon>article</mat-icon>
          </div>
          <div>
            <h2>{{ isEditing ? 'Post details' : 'What do you want to say?' }}</h2>
            <p>Content, target platforms and optional campaign.</p>
          </div>
        </div>

        <mat-form-field appearance="outline">
          <mat-label>Post content</mat-label>
          <textarea matInput [(ngModel)]="postText" name="text" rows="6" placeholder="Write your post content here..."></textarea>
          <mat-hint align="end">{{ postText.length }} / 500</mat-hint>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Target platforms</mat-label>
          <mat-select [(ngModel)]="selectedPlatforms" name="platforms" multiple>
            <mat-option *ngFor="let p of platforms" [value]="p.key">{{ p.label }}</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Campaign (optional)</mat-label>
          <mat-select [(ngModel)]="selectedCampaign" name="campaign">
            <mat-option [value]="null">None</mat-option>
            <mat-option *ngFor="let campaign of campaigns" [value]="campaign.id">{{ campaign.name }}</mat-option>
          </mat-select>
        </mat-form-field>
      </mat-card>

      <mat-card class="pf-card pf-form__card">
        <div class="pf-form__title">
          <div class="pf-feature-icon">
            <mat-icon>people</mat-icon>
          </div>
          <div>
            <h2>People & collaborators</h2>
            <p>Tag people or add collaborators on the platforms that support it (e.g. Facebook, Instagram).</p>
          </div>
        </div>

        <div class="pf-tags-editor">
          <div class="pf-tags-editor__row" *ngFor="let tag of postTags; let i = index">
            <mat-form-field appearance="outline">
              <mat-label>Platform</mat-label>
              <mat-select [(ngModel)]="tag.platform">
                <mat-option *ngFor="let p of platforms" [value]="p.key">{{ p.label }}</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Tag type</mat-label>
              <mat-select [(ngModel)]="tag.tagType">
                <mat-option *ngFor="let t of tagTypes" [value]="t.value">{{ t.label }}</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Username</mat-label>
              <input matInput [(ngModel)]="tag.username" placeholder="@username" [ngModelOptions]="{ updateOn: 'blur' }" />
            </mat-form-field>

            <button mat-icon-button (click)="removeTag(i)" aria-label="Remove tag">
              <mat-icon>close</mat-icon>
            </button>
          </div>

          <div class="pf-tags-editor__empty" *ngIf="!postTags.length">
            <p>No people tagged yet. Add a mention, a photo tag or a collaborator for a platform.</p>
          </div>

          <button mat-stroked-button (click)="addTag()">
            <mat-icon>person_add</mat-icon>
            Add tag / collaborator
          </button>
        </div>
      </mat-card>

      <mat-card class="pf-card pf-form__card">
        <div class="pf-form__title">
          <div class="pf-feature-icon">
            <mat-icon>schedule</mat-icon>
          </div>
          <div>
            <h2>Schedule</h2>
            <p>Optionally pick a platform and a time to publish automatically.</p>
          </div>
        </div>

        <div class="pf-form-row">
          <mat-form-field appearance="outline">
            <mat-label>Platform</mat-label>
            <mat-select [(ngModel)]="schedulePlatform">
              <mat-option *ngFor="let p of platforms" [value]="p.key">{{ p.label }}</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Publish at</mat-label>
            <input matInput type="datetime-local" [(ngModel)]="scheduleAt" />
          </mat-form-field>
        </div>
      </mat-card>

      <mat-card class="pf-card pf-form-actions">
        <button mat-button (click)="cancel()">Cancel</button>
        <button mat-flat-button class="pf-btn-primary" (click)="save()" [disabled]="saving || !postText.trim()">
          <mat-icon>{{ saving ? 'hourglass_top' : 'save' }}</mat-icon>
          {{ isEditing ? 'Update' : 'Save Draft' }}
        </button>
        <button mat-flat-button class="pf-btn-primary" (click)="saveAndSchedule()" [disabled]="saving || !postText.trim()">
          <mat-icon>schedule</mat-icon>
          Save & Schedule
        </button>
      </mat-card>
    </div>
  `
})
export class PostFormComponent implements OnInit {
  isEditing = false;
  postId = '';
  postText = '';
  selectedPlatforms: string[] = ['FACEBOOK'];
  selectedCampaign: string | null = null;
  schedulePlatform = 'FACEBOOK';
  scheduleAt = '';
  saving = false;
  error: string | null = null;

  campaigns: { id: string; name: string }[] = [];
  existingMediaAssets: Post['mediaAssets'] = [];
  platforms = PLATFORM_OPTIONS;

  tagTypes: TagTypeOption[] = [
    { value: PostTagType.Mention, label: 'Mention (@)' },
    { value: PostTagType.UserTag, label: 'Tag on photo' },
    { value: PostTagType.Collaborator, label: 'Collaborator' }
  ];

  postTags: PostTag[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: ApiService
  ) {
    const id = this.route.snapshot.paramMap.get('id');
    this.isEditing = !!id;
    this.postId = id ?? '';
  }

  ngOnInit(): void {
    const campaign$ = this.api.getCampaigns();
    if (this.isEditing) {
      forkJoin({
        campaigns: campaign$,
        post: this.api.getPost(this.postId)
      }).subscribe({
        next: ({ campaigns, post }) => this.prefill(campaigns, post),
        error: () => (this.error = 'Unable to load the post.')
      });
    } else {
      campaign$.subscribe({
        next: (campaigns) => {
          this.campaigns = campaigns.map((c) => ({ id: c.id, name: c.name }));
        },
        error: () => (this.error = 'Unable to load campaigns.')
      });
    }
  }

  private prefill(campaigns: { id: string; name: string }[], post: Post): void {
    this.campaigns = campaigns.map((c) => ({ id: c.id, name: c.name }));
    this.postText = post.text;
    this.selectedPlatforms = [...post.targetPlatforms];
    this.selectedCampaign = post.campaignId ?? null;
    this.postTags = post.tags.map((t) => ({ ...t }));
    this.existingMediaAssets = post.mediaAssets ?? [];
  }

  addTag(): void {
    const platform = this.selectedPlatforms[0] ?? this.platforms[0].key;
    this.postTags.push({ platform, tagType: PostTagType.Mention, username: '' });
  }

  removeTag(index: number): void {
    this.postTags.splice(index, 1);
  }

  private buildTags(): PostTag[] {
    return this.postTags
      .filter((t) => t.username.trim().length > 0)
      .map((t) => ({ platform: t.platform, tagType: t.tagType, username: t.username.trim() }));
  }

  cancel(): void {
    this.router.navigate(['/posts']);
  }

  save(): void {
    if (this.saving || !this.postText.trim()) {
      return;
    }
    this.saving = true;
    this.error = null;
    this.persistPost().pipe(finalize(() => (this.saving = false))).subscribe({
      next: () => this.router.navigate(['/posts']),
      error: () => {
        this.error = 'Unable to save the post.';
      }
    });
  }

  saveAndSchedule(): void {
    if (this.saving || !this.postText.trim()) {
      return;
    }
    if (!this.schedulePlatform || !this.scheduleAt) {
      this.error = 'Pick a platform and a date/time to schedule.';
      return;
    }
    this.saving = true;
    this.error = null;
    this.persistPost()
      .pipe(
        switchMap((postId) =>
          this.api.schedulePost({
            postId,
            platform: this.schedulePlatform,
            scheduledAtUtc: new Date(this.scheduleAt).toISOString()
          })
        ),
        finalize(() => (this.saving = false))
      )
      .subscribe({
        next: () => this.router.navigate(['/scheduling']),
        error: () => {
          this.error = 'Unable to save and schedule the post.';
        }
      });
  }

  private persistPost() {
    const tags = this.buildTags();
    if (this.isEditing) {
      const payload: UpdatePostRequest = {
        id: this.postId,
        text: this.postText.trim(),
        mediaAssets: this.existingMediaAssets,
        targetPlatforms: this.selectedPlatforms,
        tags
      };
      return this.api.updatePost(this.postId, payload).pipe(map(() => this.postId));
    }
    const payload: CreatePostRequest = {
      text: this.postText.trim(),
      mediaAssetIds: [],
      targetPlatforms: this.selectedPlatforms,
      campaignId: this.selectedCampaign ?? undefined,
      tags
    };
    return this.api.createPost(payload);
  }
}