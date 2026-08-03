import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CampaignGoal, CampaignChannel } from '../../../models/campaign.model';

@Component({
  selector: 'app-campaign-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="pf-page pf-form">
      <div class="pf-page-header">
        <div>
          <h1 class="pf-title">{{ isEditing ? 'Edit Campaign' : 'New Campaign' }}</h1>
          <p class="pf-subtitle">Give your content a goal, a channel and a timeframe.</p>
        </div>
      </div>

      <mat-card class="pf-card pf-form__card">
        <div class="pf-form__title">
          <div class="pf-feature-icon">
            <mat-icon>campaign</mat-icon>
          </div>
          <div>
            <h2>Campaign details</h2>
            <p>Name it, describe the intent and set the runway.</p>
          </div>
        </div>

        <mat-form-field appearance="outline">
          <mat-label>Campaign name</mat-label>
          <input matInput [(ngModel)]="campaignName" placeholder="Enter campaign name">
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Description</mat-label>
          <textarea matInput [(ngModel)]="campaignDescription" rows="3" placeholder="Campaign description"></textarea>
        </mat-form-field>

        <div class="pf-form-row">
          <mat-form-field appearance="outline">
            <mat-label>Goal</mat-label>
            <mat-select [(ngModel)]="selectedGoal">
              <mat-option *ngFor="let goal of goals" [value]="goal.value">{{ goal.label }}</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Channel</mat-label>
            <mat-select [(ngModel)]="selectedChannel">
              <mat-option *ngFor="let channel of channels" [value]="channel.value">{{ channel.label }}</mat-option>
            </mat-select>
          </mat-form-field>
        </div>

        <div class="pf-form-row">
          <mat-form-field appearance="outline">
            <mat-label>Start date</mat-label>
            <input matInput [matDatepicker]="startPicker" [(ngModel)]="startDate">
            <mat-datepicker-toggle matIconSuffix [for]="startPicker"></mat-datepicker-toggle>
            <mat-datepicker #startPicker></mat-datepicker>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>End date</mat-label>
            <input matInput [matDatepicker]="endPicker" [(ngModel)]="endDate">
            <mat-datepicker-toggle matIconSuffix [for]="endPicker"></mat-datepicker-toggle>
            <mat-datepicker #endPicker></mat-datepicker>
          </mat-form-field>
        </div>
      </mat-card>

      <mat-card class="pf-card pf-form-actions">
        <button mat-button (click)="cancel()">Cancel</button>
        <button mat-flat-button class="pf-btn-primary" (click)="save()">
          <mat-icon>save</mat-icon>
          {{ isEditing ? 'Update' : 'Create' }}
        </button>
      </mat-card>
    </div>
  `
})
export class CampaignFormComponent {
  isEditing = false;
  campaignName = '';
  campaignDescription = '';
  selectedGoal: CampaignGoal = CampaignGoal.Awareness;
  selectedChannel: CampaignChannel = CampaignChannel.Organic;
  startDate: Date | null = new Date();
  endDate: Date | null = new Date(Date.now() + 86400000 * 30);

  goals = [
    { value: CampaignGoal.Awareness, label: 'Awareness' },
    { value: CampaignGoal.Reputation, label: 'Reputation' },
    { value: CampaignGoal.LeadGeneration, label: 'Lead Generation' }
  ];

  channels = [
    { value: CampaignChannel.Organic, label: 'Organic' },
    { value: CampaignChannel.Paid, label: 'Paid' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.isEditing = !!this.route.snapshot.paramMap.get('id');
  }

  cancel(): void {
    this.router.navigate(['/campaigns']);
  }

  save(): void {
    this.router.navigate(['/campaigns']);
  }
}
