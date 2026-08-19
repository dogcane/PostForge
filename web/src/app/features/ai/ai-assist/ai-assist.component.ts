import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { finalize } from 'rxjs';
import { CaptionResult, ImageResult } from '../../../models/ai.model';
import { PLATFORM_OPTIONS } from '../../../models/platform.model';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-ai-assist',
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
    <div class="pf-page">
      <div class="pf-page-header">
        <div>
          <h1 class="pf-title">AI Assist</h1>
          <p class="pf-subtitle">Generate or polish your content with your own AI providers</p>
        </div>
      </div>

      <div class="pf-alert" *ngIf="error">{{ error }}</div>

      <div class="pf-grid pf-grid--ai">
        <mat-card class="pf-card pf-ai-card">
          <div class="pf-ai-card__header">
            <div class="pf-feature-icon">
              <mat-icon>psychology</mat-icon>
            </div>
            <div>
              <h2>Generate caption</h2>
              <p>Turn a brief into a ready-to-publish caption</p>
            </div>
          </div>

          <mat-form-field appearance="outline">
            <mat-label>Content brief</mat-label>
            <textarea matInput [(ngModel)]="captionBrief" rows="4" placeholder="Describe what you want the post to say..."></textarea>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Tone</mat-label>
            <mat-select [(ngModel)]="selectedTone">
              <mat-option value="professional">Professional</mat-option>
              <mat-option value="casual">Casual</mat-option>
              <mat-option value="humorous">Humorous</mat-option>
              <mat-option value="inspirational">Inspirational</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Platform constraints (optional)</mat-label>
            <mat-select [(ngModel)]="selectedPlatform">
              <mat-option [value]="null">Generic</mat-option>
              <mat-option *ngFor="let p of platforms" [value]="p.key">{{ p.label }}</mat-option>
            </mat-select>
          </mat-form-field>

          <div class="pf-ai-card__actions">
            <button mat-flat-button class="pf-btn-primary" (click)="generateCaption()" [disabled]="captionLoading || !captionBrief.trim()">
              <mat-icon>{{ captionLoading ? 'hourglass_top' : 'auto_awesome' }}</mat-icon>
              Generate caption
            </button>
          </div>

          <div class="pf-ai-result" *ngIf="captionResult">
            <p>{{ captionResult.caption }}</p>
            <div class="pf-ai-result__actions">
              <button mat-button (click)="copyCaption()">
                <mat-icon>content_copy</mat-icon>
                Copy
              </button>
            </div>
          </div>
        </mat-card>

        <mat-card class="pf-card pf-ai-card">
          <div class="pf-ai-card__header">
            <div class="pf-feature-icon">
              <mat-icon>image</mat-icon>
            </div>
            <div>
              <h2>Generate image</h2>
              <p>Create visuals from a text prompt</p>
            </div>
          </div>

          <mat-form-field appearance="outline">
            <mat-label>Image prompt</mat-label>
            <textarea matInput [(ngModel)]="imagePrompt" rows="4" placeholder="Describe the image you want to generate..."></textarea>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Style (optional)</mat-label>
            <input matInput [(ngModel)]="imageStyle" placeholder="e.g. minimal, neon, editorial" />
          </mat-form-field>

          <div class="pf-ai-card__actions">
            <button mat-flat-button class="pf-btn-primary" (click)="generateImage()" [disabled]="imageLoading || !imagePrompt.trim()">
              <mat-icon>{{ imageLoading ? 'hourglass_top' : 'auto_awesome' }}</mat-icon>
              Generate image
            </button>
          </div>

          <div class="pf-ai-image" *ngIf="imageResult">
            <img [src]="imageResult.blobUri" alt="Generated image" />
            <div class="pf-ai-result__actions">
              <a mat-button [href]="imageResult.blobUri" target="_blank" rel="noopener">
                <mat-icon>open_in_new</mat-icon>
                Open
              </a>
            </div>
          </div>
        </mat-card>
      </div>
    </div>
  `
})
export class AiAssistComponent {
  captionBrief = '';
  selectedTone = 'professional';
  selectedPlatform: string | null = null;
  captionLoading = false;
  captionResult: CaptionResult | null = null;

  imagePrompt = '';
  imageStyle = '';
  imageLoading = false;
  imageResult: ImageResult | null = null;

  error: string | null = null;
  platforms = PLATFORM_OPTIONS;

  constructor(private api: ApiService) {}

  generateCaption(): void {
    if (this.captionLoading || !this.captionBrief.trim()) {
      return;
    }
    this.captionLoading = true;
    this.error = null;
    this.api
      .generateCaption({
        brief: this.captionBrief.trim(),
        tone: this.selectedTone,
        platform: this.selectedPlatform ?? undefined
      })
      .pipe(finalize(() => (this.captionLoading = false)))
      .subscribe({
        next: (result) => {
          this.captionResult = result;
          this.imageResult = null;
        },
        error: () => {
          this.error = 'Unable to generate the caption. Check that your AI provider is configured.';
        }
      });
  }

  generateImage(): void {
    if (this.imageLoading || !this.imagePrompt.trim()) {
      return;
    }
    this.imageLoading = true;
    this.error = null;
    this.api
      .generateImage({
        prompt: this.imagePrompt.trim(),
        style: this.imageStyle.trim() || undefined
      })
      .pipe(finalize(() => (this.imageLoading = false)))
      .subscribe({
        next: (result) => {
          this.imageResult = result;
          this.captionResult = null;
        },
        error: () => {
          this.error = 'Unable to generate the image. Check that your AI provider is configured.';
        }
      });
  }

  copyCaption(): void {
    if (this.captionResult?.caption) {
      navigator.clipboard.writeText(this.captionResult.caption);
    }
  }
}