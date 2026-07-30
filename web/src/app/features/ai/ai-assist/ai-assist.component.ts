import { Component } from '@angular/core';
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
    MatIconModule,
    MatChipsModule
  ],
  template: `
    <div class="ai-container">
      <h1>AI Assist</h1>
      <p class="subtitle">Generate or improve your content with AI assistance</p>

      <div class="ai-grid">
        <mat-card class="ai-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>auto_awesome</mat-icon>
            <mat-card-title>Generate Caption</mat-card-title>
            <mat-card-subtitle>Create a caption from a brief description</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Content Brief</mat-label>
              <textarea matInput [(ngModel)]="captionBrief" rows="4" placeholder="Describe what you want the post to say..."></textarea>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Tone</mat-label>
              <mat-select [(ngModel)]="selectedTone">
                <mat-option value="professional">Professional</mat-option>
                <mat-option value="casual">Casual</mat-option>
                <mat-option value="humorous">Humorous</mat-option>
                <mat-option value="inspirational">Inspirational</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>AI Provider</mat-label>
              <mat-select [(ngModel)]="selectedTextProvider">
                <mat-option value="openai">OpenAI</mat-option>
                <mat-option value="anthropic">Anthropic</mat-option>
                <mat-option value="gemini">Google Gemini</mat-option>
                <mat-option value="foundry">Microsoft Foundry</mat-option>
              </mat-select>
            </mat-form-field>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color="primary" (click)="generateCaption()" [disabled]="!captionBrief">
              <mat-icon>psychology</mat-icon>
              Generate Caption
            </button>
          </mat-card-actions>
        </mat-card>

        <mat-card class="ai-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>image</mat-icon>
            <mat-card-title>Generate Image</mat-card-title>
            <mat-card-subtitle>Create an image from a text prompt</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Image Prompt</mat-label>
              <textarea matInput [(ngModel)]="imagePrompt" rows="4" placeholder="Describe the image you want to generate..."></textarea>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>AI Provider</mat-label>
              <mat-select [(ngModel)]="selectedImageProvider">
                <mat-option value="openai">OpenAI (DALL-E)</mat-option>
                <mat-option value="foundry">Microsoft Foundry</mat-option>
              </mat-select>
            </mat-form-field>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color="primary" (click)="generateImage()" [disabled]="!imagePrompt">
              <mat-icon>auto_awesome</mat-icon>
              Generate Image
            </button>
          </mat-card-actions>
        </mat-card>
      </div>

      <mat-card class="result-card" *ngIf="generatedContent">
        <mat-card-header>
          <mat-icon mat-card-avatar>check_circle</mat-icon>
          <mat-card-title>Generated Content</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <p>{{ generatedContent }}</p>
        </mat-card-content>
        <mat-card-actions align="end">
          <button mat-button (click)="copyContent()">
            <mat-icon>content_copy</mat-icon>
            Copy
          </button>
          <button mat-raised-button color="primary">
            <mat-icon>add</mat-icon>
            Use in New Post
          </button>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: [`
    .ai-container h1 {
      font-weight: 500;
      margin-bottom: 4px;
    }
    .subtitle {
      color: rgba(0, 0, 0, 0.54);
      margin-bottom: 24px;
    }
    .ai-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
      margin-bottom: 24px;
    }
    .ai-card {
      height: fit-content;
    }
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }
    .result-card {
      margin-top: 16px;
    }
    @media (max-width: 768px) {
      .ai-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class AiAssistComponent {
  captionBrief = '';
  selectedTone = 'professional';
  selectedTextProvider = 'openai';
  imagePrompt = '';
  selectedImageProvider = 'openai';
  generatedContent: string | null = null;

  generateCaption(): void {
    this.generatedContent = 'Generated caption based on your brief with ' + this.selectedTone + ' tone using ' + this.selectedTextProvider + '.';
  }

  generateImage(): void {
    this.generatedContent = 'Image will be generated from your prompt using ' + this.selectedImageProvider + '.';
  }

  copyContent(): void {
    if (this.generatedContent) {
      navigator.clipboard.writeText(this.generatedContent);
    }
  }
}
