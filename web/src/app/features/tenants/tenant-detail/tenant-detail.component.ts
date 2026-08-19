import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { finalize } from 'rxjs';
import { Tenant, TenantUser } from '../../../models/tenant.model';
import { TenantService } from '../../../services/tenant.service';

@Component({
  selector: 'app-tenant-detail',
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
    <div class="pf-page">
      <div class="pf-page-header">
        <div>
          <h1 class="pf-title">{{ tenant?.name ?? 'Tenant' }}</h1>
          <p class="pf-subtitle">Manage members and access to this workspace.</p>
        </div>
        <a mat-button routerLink="/tenants">
          <mat-icon>arrow_back</mat-icon>
          All tenants
        </a>
      </div>

      <div class="pf-alert" *ngIf="error">{{ error }}</div>

      <mat-card class="pf-card pf-form__card" *ngIf="tenant">
        <div class="pf-form__title">
          <div class="pf-feature-icon">
            <mat-icon>domain</mat-icon>
          </div>
          <div>
            <h2>Tenant</h2>
            <p>
              <span class="pf-tag">{{ tenant.slug }}</span>
              <span class="pf-channel" [class]="tenant.isActive ? 'pf-channel--organic' : 'pf-channel--paid'">
                {{ tenant.isActive ? 'Active' : 'Inactive' }}
              </span>
            </p>
          </div>
        </div>
      </mat-card>

      <mat-card class="pf-card pf-form__card">
        <div class="pf-form__title">
          <div class="pf-feature-icon">
            <mat-icon>group</mat-icon>
          </div>
          <div>
            <h2>Members</h2>
            <p>Add accounts to this tenant. New users are created with a password they can change later.</p>
          </div>
        </div>

        <div class="pf-invite">
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" [(ngModel)]="newEmail" name="email" placeholder="member@example.com" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Initial password</mat-label>
            <input matInput type="password" [(ngModel)]="newPassword" name="password" placeholder="Min 8 chars, upper+lower+digit+symbol" />
          </mat-form-field>
          <button mat-flat-button class="pf-btn-primary" (click)="addUser()" [disabled]="adding || !newEmail.trim() || !newPassword">
            <mat-icon>person_add</mat-icon>
            Add member
          </button>
        </div>

        <div class="pf-invite-error" *ngIf="addError">{{ addError }}</div>

        <div class="pf-table" *ngIf="users.length">
          <div class="pf-table__row pf-table__row--head">
            <span>User</span>
            <span>Joined</span>
            <span></span>
          </div>
          <div class="pf-table__row" *ngFor="let user of users">
            <span class="pf-table__email">
              <mat-icon>account_circle</mat-icon>
              {{ user.email }}
            </span>
            <span>{{ user.joinedAtUtc | date: 'MMM d, yyyy' }}</span>
            <span class="pf-table__actions">
              <button mat-icon-button (click)="removeUser(user)" aria-label="Remove member">
                <mat-icon>person_remove</mat-icon>
              </button>
            </span>
          </div>
        </div>

        <div class="pf-table__empty" *ngIf="!loading && !users.length">
          No members yet. Add the first one above.
        </div>
      </mat-card>
    </div>
  `
})
export class TenantDetailComponent implements OnInit {
  tenantId: string;
  tenant: Tenant | null = null;
  users: TenantUser[] = [];
  newEmail = '';
  newPassword = '';
  loading = false;
  adding = false;
  error: string | null = null;
  addError: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private tenantService: TenantService
  ) {
    this.tenantId = this.route.snapshot.paramMap.get('id') ?? '';
  }

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading = true;
    this.error = null;
    this.tenantService.getTenant(this.tenantId).subscribe({
      next: (tenant) => {
        this.tenant = tenant;
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load the tenant.';
        this.loading = false;
      }
    });
    this.tenantService.getUsers(this.tenantId).subscribe({
      next: (users) => {
        this.users = users;
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load the tenant members.';
        this.loading = false;
      }
    });
  }

  addUser(): void {
    if (this.adding || !this.newEmail.trim() || !this.newPassword) {
      return;
    }
    this.adding = true;
    this.addError = null;
    this.tenantService
      .addUser({
        tenantId: this.tenantId,
        email: this.newEmail.trim(),
        password: this.newPassword
      })
      .pipe(finalize(() => (this.adding = false)))
      .subscribe({
        next: () => {
          this.newEmail = '';
          this.newPassword = '';
          this.tenantService.getUsers(this.tenantId).subscribe({
            next: (users) => (this.users = users)
          });
        },
        error: () => {
          this.addError =
            'Unable to add the member. The email may already exist or the password may not meet the policy.';
        }
      });
  }

  removeUser(user: TenantUser): void {
    if (!confirm(`Remove ${user.email} from this tenant?`)) {
      return;
    }
    this.tenantService.removeUser(this.tenantId, user.userId).subscribe({
      next: () => {
        this.users = this.users.filter((u) => u.userId !== user.userId);
      },
      error: () => {
        this.addError = 'Unable to remove the member.';
      }
    });
  }
}