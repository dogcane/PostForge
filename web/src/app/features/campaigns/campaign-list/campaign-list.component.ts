import { Component, effect, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import {
  Campaign,
  campaignChannelClass,
  campaignChannelLabel,
  campaignGoalClass,
  campaignGoalLabel
} from '../../../models/campaign.model';
import { ApiService } from '../../../services/api.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-campaign-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, MatIconModule],
  template: `
    <div class="pf-page">
      <div class="pf-page-header">
        <div>
          <h1 class="pf-title">Campaigns</h1>
          <p class="pf-subtitle">Group your content around a goal and keep it on track</p>
        </div>
        <a mat-flat-button class="pf-btn-primary" routerLink="/campaigns/new">
          <mat-icon>add</mat-icon>
          New Campaign
        </a>
      </div>

      <div class="pf-alert" *ngIf="error">{{ error }}</div>

      <ng-container *ngIf="!loading; else loadingState">
        <ng-container *ngIf="campaigns.length; else empty">
          <div class="pf-grid pf-grid--campaigns">
            <article class="pf-card pf-card--hover campaign-card" *ngFor="let c of campaigns">
              <div class="campaign-card__head">
                <span class="pf-goal" [class]="campaignGoalClass(c.goal)">{{ campaignGoalLabel(c.goal) }}</span>
                <span class="pf-channel" [class]="campaignChannelClass(c.channel)">{{ campaignChannelLabel(c.channel) }}</span>
              </div>
              <h3 class="campaign-card__name">{{ c.name }}</h3>
              <div class="campaign-card__meta">
                <span>
                  <mat-icon>date_range</mat-icon>
                  {{ c.startDateUtc | date: 'MMM d' }} – {{ (c.endDateUtc | date: 'MMM d, yyyy') ?? 'Open ended' }}
                </span>
                <span class="spacer"></span>
                <span>
                  <mat-icon>article</mat-icon>
                  {{ c.postIds.length }} posts
                </span>
                <button mat-icon-button [routerLink]="['/campaigns', c.id]" aria-label="Edit campaign">
                  <mat-icon>edit</mat-icon>
                </button>
                <button mat-icon-button (click)="deleteCampaign(c)" aria-label="Delete campaign">
                  <mat-icon>delete_outline</mat-icon>
                </button>
              </div>
            </article>
          </div>
        </ng-container>
      </ng-container>

      <ng-template #empty>
        <div class="pf-empty">
          <mat-icon>campaign</mat-icon>
          <h3>No campaigns yet</h3>
          <p>Bundle posts under a goal and a channel to keep your publishing strategy focused.</p>
          <a mat-flat-button class="pf-btn-primary" routerLink="/campaigns/new">
            <mat-icon>add</mat-icon>
            Create a campaign
          </a>
        </div>
      </ng-template>

      <ng-template #loadingState>
        <div class="pf-loading"><mat-icon>autorenew</mat-icon> Loading campaigns...</div>
      </ng-template>
    </div>
  `
})
export class CampaignListComponent {
  campaigns: Campaign[] = [];
  loading = false;
  error: string | null = null;

  private readonly auth = inject(AuthService);

  constructor(private api: ApiService) {
    effect(() => {
      this.auth.activeTenantIdSignal();
      this.load();
    });
  }

  private load(): void {
    this.loading = true;
    this.error = null;
    this.api.getCampaigns().subscribe({
      next: (campaigns) => {
        this.campaigns = campaigns;
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load campaigns.';
        this.loading = false;
      }
    });
  }

  deleteCampaign(campaign: Campaign): void {
    if (!confirm(`Delete campaign "${campaign.name}"? This cannot be undone.`)) {
      return;
    }
    this.api.deleteCampaign(campaign.id).subscribe({
      next: () => this.load(),
      error: () => {
        this.error = 'Unable to delete the campaign.';
      }
    });
  }

  readonly campaignGoalClass = campaignGoalClass;
  readonly campaignGoalLabel = campaignGoalLabel;
  readonly campaignChannelClass = campaignChannelClass;
  readonly campaignChannelLabel = campaignChannelLabel;
}