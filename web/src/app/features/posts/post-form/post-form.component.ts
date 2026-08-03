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
            <mat-option value="facebook">Facebook</mat-option>
            <mat-option value="instagram">Instagram</mat-option>
            <mat-option value="tiktok">TikTok</mat-option>
            <mat-option value="youtube">YouTube</mat-option>
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

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.isEditing = !!this.route.snapshot.paramMap.get('id');
    if (this.isEditing) {
      this.postText = 'Sample draft content being edited...';
    }
  }

  cancel(): void {
    this.router.navigate(['/posts']);
  }

  save(): void {
    this.router.navigate(['/posts']);
  }

  saveAndSchedule(): void {
    this.router.navigate(['/scheduling']);
  }
}
