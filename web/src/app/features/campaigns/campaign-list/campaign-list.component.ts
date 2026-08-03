import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Campaign, CampaignGoal, CampaignChannel } from '../../../models/campaign.model';

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

      <ng-container *ngIf="campaigns.length; else empty">
        <div class="pf-grid pf-grid--campaigns">
          <article class="pf-card pf-card--hover campaign-card" *ngFor="let c of campaigns">
            <div class="campaign-card__head">
              <span class="pf-goal" [class]="goalClass(c.goal)">{{ goalLabel(c.goal) }}</span>
              <span class="pf-channel" [class]="'pf-channel--' + c.channel.toLowerCase()">{{ c.channel }}</span>
            </div>
            <h3 class="campaign-card__name">{{ c.name }}</h3>
            <p class="campaign-card__desc">{{ c.description }}</p>
            <div class="campaign-card__meta">
              <span>
                <mat-icon>date_range</mat-icon>
                {{ c.startDate | date: 'MMM d' }} – {{ c.endDate | date: 'MMM d, yyyy' }}
              </span>
              <span class="spacer"></span>
              <span>
                <mat-icon>article</mat-icon>
                {{ c.postIds.length }} posts
              </span>
              <button mat-icon-button [routerLink]="['/campaigns', c.id]" aria-label="Edit campaign">
                <mat-icon>edit</mat-icon>
              </button>
            </div>
          </article>
        </div>
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
    </div>
  `
})
export class CampaignListComponent {
  campaigns: Campaign[] = [
    {
      id: '1',
      name: 'Summer Launch',
      description: 'Cross-platform product reveal for the summer collection, with teasers and a reveal day.',
      goal: CampaignGoal.Awareness,
      channel: CampaignChannel.Organic,
      startDate: new Date(Date.now() - 86400000 * 6).toISOString(),
      endDate: new Date(Date.now() + 86400000 * 24).toISOString(),
      postIds: ['1', '2'],
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    },
    {
      id: '2',
      name: 'Lead Gen Sprint',
      description: 'Paid push on Facebook and Instagram driving sign-ups for the early access waitlist.',
      goal: CampaignGoal.LeadGeneration,
      channel: CampaignChannel.Paid,
      startDate: new Date(Date.now() + 86400000 * 3).toISOString(),
      endDate: new Date(Date.now() + 86400000 * 17).toISOString(),
      postIds: ['3'],
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    },
    {
      id: '3',
      name: 'Brand Trust Series',
      description: 'A story-driven series about how we build PostForge, one weekly episode at a time.',
      goal: CampaignGoal.Reputation,
      channel: CampaignChannel.Organic,
      startDate: new Date(Date.now() - 86400000 * 14).toISOString(),
      endDate: new Date(Date.now() + 86400000 * 35).toISOString(),
      postIds: [],
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    }
  ];

  goalClass(goal: CampaignGoal): string {
    return 'pf-goal--' + goal.toLowerCase();
  }

  goalLabel(goal: CampaignGoal): string {
    switch (goal) {
      case CampaignGoal.Awareness: return 'Awareness';
      case CampaignGoal.Reputation: return 'Reputation';
      case CampaignGoal.LeadGeneration: return 'Lead Gen';
      default: return goal;
    }
  }
}
