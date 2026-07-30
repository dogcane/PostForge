import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { Campaign, CampaignGoal, CampaignChannel } from '../../../models/campaign.model';

@Component({
  selector: 'app-campaign-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatTableModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule
  ],
  template: `
    <div class="campaign-list-header">
      <h1>Campaigns</h1>
      <button mat-raised-button color="primary" routerLink="/campaigns/new">
        <mat-icon>add</mat-icon>
        New Campaign
      </button>
    </div>

    <mat-card>
      <mat-card-content>
        <table mat-table [dataSource]="campaigns" class="campaign-table">
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let campaign">{{ campaign.name }}</td>
          </ng-container>

          <ng-container matColumnDef="goal">
            <th mat-header-cell *matHeaderCellDef>Goal</th>
            <td mat-cell *matCellDef="let campaign">
              <mat-chip [color]="getGoalColor(campaign.goal)" selected>{{ campaign.goal }}</mat-chip>
            </td>
          </ng-container>

          <ng-container matColumnDef="channel">
            <th mat-header-cell *matHeaderCellDef>Channel</th>
            <td mat-cell *matCellDef="let campaign">{{ campaign.channel }}</td>
          </ng-container>

          <ng-container matColumnDef="dateRange">
            <th mat-header-cell *matHeaderCellDef>Date Range</th>
            <td mat-cell *matCellDef="let campaign">
              {{ campaign.startDate | date:'shortDate' }} - {{ campaign.endDate | date:'shortDate' }}
            </td>
          </ng-container>

          <ng-container matColumnDef="posts">
            <th mat-header-cell *matHeaderCellDef>Posts</th>
            <td mat-cell *matCellDef="let campaign">{{ campaign.postIds.length }}</td>
          </ng-container>

          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let campaign">
              <button mat-icon-button [routerLink]="['/campaigns', campaign.id]">
                <mat-icon>edit</mat-icon>
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .campaign-list-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }
    .campaign-list-header h1 {
      margin: 0;
      font-weight: 500;
    }
    .campaign-table {
      width: 100%;
    }
  `]
})
export class CampaignListComponent {
  displayedColumns: string[] = ['name', 'goal', 'channel', 'dateRange', 'posts', 'actions'];

  campaigns: Campaign[] = [];

  getGoalColor(goal: CampaignGoal): string {
    switch (goal) {
      case CampaignGoal.Awareness: return 'primary';
      case CampaignGoal.Reputation: return 'accent';
      case CampaignGoal.LeadGeneration: return 'warn';
      default: return '';
    }
  }
}
