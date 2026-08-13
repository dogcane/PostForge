import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { PostContent, PostTag, PostTagType } from '../../../models/post.model';

interface PlatformOption {
  key: string;
  label: string;
}

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
          <textarea matInput [(ngModel)]="postText" rows="6" placeholder="Write your post content here..."></textarea>
          <mat-hint align="end">{{ postText.length }} / 500</mat-hint>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Target platforms</mat-label>
          <mat-select [(ngModel)]="selectedPlatforms" multiple>
            <mat-option *ngFor="let p of platforms" [value]="p.key">{{ p.label }}</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Campaign (optional)</mat-label>
          <mat-select [(ngModel)]="selectedCampaign">
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
              <input matInput [(ngModel)]="tag.username" placeholder="@username" [ngModelOptions]="{ updateOn: 'blur' }">
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

      <mat-card class="pf-card pf-form-actions">
        <button mat-button (click)="cancel()">Cancel</button>
        <button mat-flat-button class="pf-btn-primary" (click)="save()">
          <mat-icon>save</mat-icon>
          {{ isEditing ? 'Update' : 'Save Draft' }}
        </button>
        <button mat-flat-button class="pf-btn-primary" (click)="saveAndSchedule()">
          <mat-icon>schedule</mat-icon>
          Save & Schedule
        </button>
      </mat-card>
    </div>
  `
})
export class PostFormComponent {
  isEditing = false;
  postText = '';
  selectedPlatforms: string[] = ['facebook'];
  selectedCampaign: string | null = null;
  campaigns: { id: string; name: string }[] = [
    { id: '1', name: 'Summer Launch' },
    { id: '2', name: 'Brand Awareness Q3' }
  ];

  platforms: PlatformOption[] = [
    { key: 'facebook', label: 'Facebook' },
    { key: 'instagram', label: 'Instagram' },
    { key: 'tiktok', label: 'TikTok' },
    { key: 'youtube', label: 'YouTube' }
  ];

  tagTypes: TagTypeOption[] = [
    { value: PostTagType.Mention, label: 'Mention (@)' },
    { value: PostTagType.UserTag, label: 'Tag on photo' },
    { value: PostTagType.Collaborator, label: 'Collaborator' }
  ];

  postTags: PostTag[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.isEditing = !!this.route.snapshot.paramMap.get('id');
    if (this.isEditing) {
      this.postText = 'Sample draft content being edited...';
      this.postTags = [
        { platform: 'facebook', tagType: PostTagType.Collaborator, username: 'silvia.neri' },
        { platform: 'instagram', tagType: PostTagType.Mention, username: 'marco.rossi' }
      ];
    }
  }

  addTag(): void {
    const platform = this.selectedPlatforms[0] ?? this.platforms[0].key;
    this.postTags.push({ platform, tagType: PostTagType.Mention, username: '' });
  }

  removeTag(index: number): void {
    this.postTags.splice(index, 1);
  }

  buildPayload(): PostContent {
    return {
      text: this.postText,
      mediaIds: [],
      platformKeys: this.selectedPlatforms,
      campaignId: this.selectedCampaign ?? undefined,
      tags: this.postTags.filter(t => t.username.trim().length > 0)
    };
  }

  cancel(): void {
    this.router.navigate(['/posts']);
  }

  save(): void {
    const payload = this.buildPayload();
    // TODO(wire): POST /api/v1/posts with the payload above once the API integration lands.
    this.router.navigate(['/posts']);
  }

  saveAndSchedule(): void {
    const payload = this.buildPayload();
    // TODO(wire): create the post, then open scheduling for the returned id.
    this.router.navigate(['/scheduling']);
  }
}
