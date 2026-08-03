import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

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
            <mat-label>AI provider</mat-label>
            <mat-select [(ngModel)]="selectedTextProvider">
              <mat-option value="openai">OpenAI</mat-option>
              <mat-option value="anthropic">Anthropic</mat-option>
              <mat-option value="gemini">Google Gemini</mat-option>
              <mat-option value="foundry">Microsoft Foundry</mat-option>
            </mat-select>
          </mat-form-field>

          <div class="pf-ai-card__actions">
            <button mat-flat-button class="pf-btn-primary" (click)="generateCaption()" [disabled]="!captionBrief">
              <mat-icon>auto_awesome</mat-icon>
              Generate caption
            </button>
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
            <mat-label>AI provider</mat-label>
            <mat-select [(ngModel)]="selectedImageProvider">
              <mat-option value="openai">OpenAI (DALL-E)</mat-option>
              <mat-option value="foundry">Microsoft Foundry</mat-option>
            </mat-select>
          </mat-form-field>

          <div class="pf-ai-card__actions">
            <button mat-flat-button class="pf-btn-primary" (click)="generateImage()" [disabled]="!imagePrompt">
              <mat-icon>auto_awesome</mat-icon>
              Generate image
            </button>
          </div>
        </mat-card>
      </div>

      <mat-card class="pf-card pf-result" *ngIf="generatedContent">
        <div class="pf-result__head">
          <mat-icon>check_circle</mat-icon>
          Generated content
        </div>
        <p class="pf-result__text">{{ generatedContent }}</p>
        <div class="pf-form-actions" style="padding:0">
          <button mat-button (click)="copyContent()">
            <mat-icon>content_copy</mat-icon>
            Copy
          </button>
          <button mat-flat-button class="pf-btn-primary">
            <mat-icon>add</mat-icon>
            Use in New Post
          </button>
        </div>
      </mat-card>
    </div>
  `
})
export class AiAssistComponent {
  captionBrief = '';
  selectedTone = 'professional';
  selectedTextProvider = 'openai';
  imagePrompt = '';
  selectedImageProvider = 'openai';
  generatedContent: string | null = null;

  generateCaption(): void {
    this.generatedContent =
      'Generated caption based on your brief with a ' +
      this.selectedTone +
      ' tone using ' +
      this.selectedTextProvider +
      '.';
  }

  generateImage(): void {
    this.generatedContent =
      'Image will be generated from your prompt using ' + this.selectedImageProvider + '.';
  }

  copyContent(): void {
    if (this.generatedContent) {
      navigator.clipboard.writeText(this.generatedContent);
    }
  }
}
