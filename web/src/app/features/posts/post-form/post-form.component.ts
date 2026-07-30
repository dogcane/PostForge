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
import { MatChipsModule } from '@angular/material/chips';

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
    MatIconModule,
    MatChipsModule
  ],
  template: `
    <div class="form-container">
      <h1>{{ isEditing ? 'Edit Post' : 'New Post' }}</h1>

      <mat-card>
        <mat-card-content>
          <div class="form-fields">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Post Content</mat-label>
              <textarea matInput [(ngModel)]="postText" rows="6" placeholder="Write your post content here..."></textarea>
              <mat-hint align="end">{{ postText.length }} / 500</mat-hint>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Target Platforms</mat-label>
              <mat-select [(ngModel)]="selectedPlatforms" multiple>
                <mat-option value="facebook">Facebook</mat-option>
                <mat-option value="instagram">Instagram</mat-option>
                <mat-option value="tiktok">TikTok</mat-option>
                <mat-option value="youtube">YouTube</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Campaign (optional)</mat-label>
              <mat-select [(ngModel)]="selectedCampaign">
                <mat-option [value]="null">None</mat-option>
                <mat-option *ngFor="let campaign of campaigns" [value]="campaign.id">{{ campaign.name }}</mat-option>
              </mat-select>
            </mat-form-field>
          </div>
        </mat-card-content>
        <mat-card-actions align="end">
          <button mat-button (click)="cancel()">Cancel</button>
          <button mat-raised-button color="primary" (click)="save()">
            <mat-icon>save</mat-icon>
            {{ isEditing ? 'Update' : 'Save Draft' }}
          </button>
          <button mat-raised-button color="accent" (click)="saveAndSchedule()">
            <mat-icon>schedule</mat-icon>
            Save & Schedule
          </button>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: [`
    .form-container {
      max-width: 800px;
      margin: 0 auto;
    }
    .form-container h1 {
      font-weight: 500;
      margin-bottom: 20px;
    }
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }
    .form-fields {
      padding: 16px 0;
    }
  `]
})
export class PostFormComponent {
  isEditing = false;
  postText = '';
  selectedPlatforms: string[] = [];
  selectedCampaign: string | null = null;
  campaigns: { id: string; name: string }[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.isEditing = !!this.route.snapshot.paramMap.get('id');
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
