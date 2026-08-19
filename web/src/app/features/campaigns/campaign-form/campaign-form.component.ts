import { Component, OnInit } from '@angular/core';
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
import { Observable, finalize } from 'rxjs';
import { CampaignGoal, CampaignChannel } from '../../../models/campaign.model';
import { ApiService } from '../../../services/api.service';

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

      <div class="pf-alert" *ngIf="error">{{ error }}</div>

      <mat-card class="pf-card pf-form__card">
        <div class="pf-form__title">
          <div class="pf-feature-icon">
            <mat-icon>campaign</mat-icon>
          </div>
          <div>
            <h2>Campaign details</h2>
            <p>Name it, set the intent and choose the runway.</p>
          </div>
        </div>

        <mat-form-field appearance="outline">
          <mat-label>Campaign name</mat-label>
          <input matInput [(ngModel)]="campaignName" name="name" placeholder="Enter campaign name" />
        </mat-form-field>

        <div class="pf-form-row">
          <mat-form-field appearance="outline">
            <mat-label>Goal</mat-label>
            <mat-select [(ngModel)]="selectedGoal" name="goal">
              <mat-option *ngFor="let goal of goals" [value]="goal.value">{{ goal.label }}</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Channel</mat-label>
            <mat-select [(ngModel)]="selectedChannel" name="channel">
              <mat-option *ngFor="let channel of channels" [value]="channel.value">{{ channel.label }}</mat-option>
            </mat-select>
          </mat-form-field>
        </div>

        <div class="pf-form-row">
          <mat-form-field appearance="outline">
            <mat-label>Start date</mat-label>
            <input matInput [matDatepicker]="startPicker" [(ngModel)]="startDate" name="startDate" />
            <mat-datepicker-toggle matIconSuffix [for]="startPicker"></mat-datepicker-toggle>
            <mat-datepicker #startPicker></mat-datepicker>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>End date (optional)</mat-label>
            <input matInput [matDatepicker]="endPicker" [(ngModel)]="endDate" name="endDate" />
            <mat-datepicker-toggle matIconSuffix [for]="endPicker"></mat-datepicker-toggle>
            <mat-datepicker #endPicker></mat-datepicker>
          </mat-form-field>
        </div>
      </mat-card>

      <mat-card class="pf-card pf-form-actions">
        <button mat-button (click)="cancel()">Cancel</button>
        <button mat-flat-button class="pf-btn-primary" (click)="save()" [disabled]="saving || !campaignName.trim()">
          <mat-icon>{{ saving ? 'hourglass_top' : 'save' }}</mat-icon>
          {{ isEditing ? 'Update' : 'Create' }}
        </button>
      </mat-card>
    </div>
  `
})
export class CampaignFormComponent implements OnInit {
  isEditing = false;
  campaignId = '';
  campaignName = '';
  selectedGoal: CampaignGoal = CampaignGoal.Awareness;
  selectedChannel: CampaignChannel = CampaignChannel.Organic;
  startDate: Date | null = new Date();
  endDate: Date | null = null;
  saving = false;
  error: string | null = null;

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
    private router: Router,
    private api: ApiService
  ) {
    const id = this.route.snapshot.paramMap.get('id');
    this.isEditing = !!id;
    this.campaignId = id ?? '';
  }

  ngOnInit(): void {
    if (this.isEditing) {
      this.api.getCampaign(this.campaignId).subscribe({
        next: (campaign) => {
          this.campaignName = campaign.name;
          this.selectedGoal = campaign.goal;
          this.selectedChannel = campaign.channel;
          this.startDate = new Date(campaign.startDateUtc);
          this.endDate = campaign.endDateUtc ? new Date(campaign.endDateUtc) : null;
        },
        error: () => (this.error = 'Unable to load the campaign.')
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/campaigns']);
  }

  save(): void {
    if (this.saving || !this.campaignName.trim()) {
      return;
    }
    if (!this.startDate) {
      this.error = 'A start date is required.';
      return;
    }

    this.saving = true;
    this.error = null;

    const payload = {
      name: this.campaignName.trim(),
      goal: this.selectedGoal,
      channel: this.selectedChannel,
      startDateUtc: this.startDate.toISOString(),
      endDateUtc: this.endDate ? this.endDate.toISOString() : undefined
    };

    let request: Observable<unknown>;
    if (this.isEditing) {
      request = this.api.updateCampaign(this.campaignId, { ...payload, id: this.campaignId });
    } else {
      request = this.api.createCampaign(payload);
    }

    request.pipe(finalize(() => (this.saving = false))).subscribe({
      next: () => this.router.navigate(['/campaigns']),
      error: () => {
        this.error = 'Unable to save the campaign.';
      }
    });
  }
}