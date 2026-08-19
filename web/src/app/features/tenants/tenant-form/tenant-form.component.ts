import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { finalize } from 'rxjs';
import { TenantService } from '../../../services/tenant.service';

@Component({
  selector: 'app-tenant-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="pf-page pf-form">
      <div class="pf-page-header">
        <div>
          <h1 class="pf-title">New Tenant</h1>
          <p class="pf-subtitle">Create a workspace to scope users and content.</p>
        </div>
      </div>

      <mat-card class="pf-card pf-form__card">
        <div class="pf-form__title">
          <div class="pf-feature-icon">
            <mat-icon>domain</mat-icon>
          </div>
          <div>
            <h2>Tenant details</h2>
            <p>The slug is used as a unique identifier and can only contain lowercase letters, numbers and dashes.</p>
          </div>
        </div>

        <mat-form-field appearance="outline">
          <mat-label>Tenant name</mat-label>
          <input matInput [(ngModel)]="name" name="name" placeholder="Acme Media" (blur)="suggestSlug()" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Slug</mat-label>
          <input matInput [(ngModel)]="slug" name="slug" placeholder="acme-media" />
          <mat-hint>^[a-z0-9-]+$</mat-hint>
        </mat-form-field>

        <div class="pf-alert" *ngIf="error">{{ error }}</div>
      </mat-card>

      <mat-card class="pf-card pf-form-actions">
        <button mat-button (click)="cancel()">Cancel</button>
        <button mat-flat-button class="pf-btn-primary" (click)="save()" [disabled]="loading || !name.trim() || !slug.trim()">
          <mat-icon>{{ loading ? 'hourglass_top' : 'add' }}</mat-icon>
          Create tenant
        </button>
      </mat-card>
    </div>
  `
})
export class TenantFormComponent {
  name = '';
  slug = '';
  loading = false;
  error: string | null = null;

  constructor(
    private tenantService: TenantService,
    private router: Router
  ) {}

  suggestSlug(): void {
    if (this.slug.trim()) {
      return;
    }
    this.slug = this.name
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }

  cancel(): void {
    this.router.navigate(['/tenants']);
  }

  save(): void {
    if (this.loading || !this.name.trim() || !this.slug.trim()) {
      return;
    }

    this.loading = true;
    this.error = null;
    this.tenantService
      .createTenant({ name: this.name.trim(), slug: this.slug.trim() })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => this.router.navigate(['/tenants']),
        error: () => {
          this.error = 'Unable to create the tenant. Check the slug format and try again.';
        }
      });
  }
}