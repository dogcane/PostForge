import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Tenant } from '../../../models/tenant.model';
import { TenantService } from '../../../services/tenant.service';

@Component({
  selector: 'app-tenant-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatIconModule],
  template: `
    <div class="pf-page">
      <div class="pf-page-header">
        <div>
          <h1 class="pf-title">Tenants</h1>
          <p class="pf-subtitle">Workspaces you manage. Each tenant scopes its own content.</p>
        </div>
        <a mat-flat-button class="pf-btn-primary" routerLink="/tenants/new">
          <mat-icon>add</mat-icon>
          New Tenant
        </a>
      </div>

      <div class="pf-alert" *ngIf="error">{{ error }}</div>

      <ng-container *ngIf="!loading; else loadingState">
        <ng-container *ngIf="tenants.length; else empty">
          <div class="pf-grid pf-grid--tenants">
            <article class="pf-card pf-card--hover tenant-card" *ngFor="let tenant of tenants">
              <div class="tenant-card__head">
                <div class="pf-feature-icon pf-feature-icon--sm">
                  <mat-icon>domain</mat-icon>
                </div>
                <span class="pf-channel" [class]="tenant.isActive ? 'pf-channel--organic' : 'pf-channel--paid'">
                  {{ tenant.isActive ? 'Active' : 'Inactive' }}
                </span>
              </div>
              <h3 class="tenant-card__name">{{ tenant.name }}</h3>
              <p class="tenant-card__slug">{{ tenant.slug }}</p>
              <div class="tenant-card__meta">
                <span>
                  <mat-icon>calendar_today</mat-icon>
                  Created {{ tenant.createdAtUtc | date: 'MMM d, yyyy' }}
                </span>
                <span class="spacer"></span>
                <a mat-icon-button [routerLink]="['/tenants', tenant.id]" aria-label="Manage tenant">
                  <mat-icon>manage_accounts</mat-icon>
                </a>
              </div>
            </article>
          </div>
        </ng-container>
      </ng-container>

      <ng-template #empty>
        <div class="pf-empty">
          <mat-icon>domain</mat-icon>
          <h3>No tenants yet</h3>
          <p>Create a workspace to organize users and their content.</p>
          <a mat-flat-button class="pf-btn-primary" routerLink="/tenants/new">
            <mat-icon>add</mat-icon>
            Create a tenant
          </a>
        </div>
      </ng-template>

      <ng-template #loadingState>
        <div class="pf-loading"><mat-icon>autorenew</mat-icon> Loading tenants...</div>
      </ng-template>
    </div>
  `
})
export class TenantListComponent implements OnInit {
  tenants: Tenant[] = [];
  loading = false;
  error: string | null = null;

  constructor(private tenantService: TenantService) {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading = true;
    this.error = null;
    this.tenantService.getTenants().subscribe({
      next: (tenants) => {
        this.tenants = tenants;
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load tenants.';
        this.loading = false;
      }
    });
  }
}