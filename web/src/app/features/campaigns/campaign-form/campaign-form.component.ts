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
    <div class="form-container">
      <h1>{{ isEditing ? 'Edit Campaign' : 'New Campaign' }}</h1>

      <mat-card>
        <mat-card-content>
          <div class="form-fields">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Campaign Name</mat-label>
              <input matInput [(ngModel)]="campaignName" placeholder="Enter campaign name">
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Description</mat-label>
              <textarea matInput [(ngModel)]="campaignDescription" rows="3" placeholder="Campaign description"></textarea>
            </mat-form-field>

            <div class="form-row">
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

            <div class="form-row">
              <mat-form-field appearance="outline">
                <mat-label>Start Date</mat-label>
                <input matInput [matDatepicker]="startPicker" [(ngModel)]="startDate">
                <mat-datepicker-toggle matSuffix [for]="startPicker"></mat-datepicker-toggle>
                <mat-datepicker #startPicker></mat-datepicker>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>End Date</mat-label>
                <input matInput [matDatepicker]="endPicker" [(ngModel)]="endDate">
                <mat-datepicker-toggle matSuffix [for]="endPicker"></mat-datepicker-toggle>
                <mat-datepicker #endPicker></mat-datepicker>
              </mat-form-field>
            </div>
          </div>
        </mat-card-content>
        <mat-card-actions align="end">
          <button mat-button (click)="cancel()">Cancel</button>
          <button mat-raised-button color="primary" (click)="save()">
            <mat-icon>save</mat-icon>
            {{ isEditing ? 'Update' : 'Create' }}
          </button>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: [`
    .form-container {
      max-width: 700px;
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
    .form-row {
      display: flex;
      gap: 16px;
      margin-bottom: 16px;
    }
    .form-row mat-form-field {
      flex: 1;
    }
    .form-fields {
      padding: 16px 0;
    }
  `]
})
export class CampaignFormComponent {
  isEditing = false;
  campaignName = '';
  campaignDescription = '';
  selectedGoal: CampaignGoal = CampaignGoal.Awareness;
  selectedChannel: CampaignChannel = CampaignChannel.Organic;
  startDate: Date | null = null;
  endDate: Date | null = null;

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
