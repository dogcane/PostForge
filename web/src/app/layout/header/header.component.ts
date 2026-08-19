import { Component, EventEmitter, Output, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatSelectModule, MatFormFieldModule, MatButtonModule],
  template: `
    <header class="pf-header">
      <button class="pf-icon-btn" (click)="toggleMenu.emit()" aria-label="Toggle navigation">
        <mat-icon>menu</mat-icon>
      </button>

      <div class="pf-search">
        <mat-icon>search</mat-icon>
        <input type="text" placeholder="Search posts, campaigns..." />
        <kbd>Ctrl K</kbd>
      </div>

      <div class="pf-header__actions">
        <mat-form-field *ngIf="canSwitchTenant()" class="pf-tenant-select" appearance="outline">
          <mat-label>Workspace</mat-label>
          <mat-select [value]="activeTenantId()" (selectionChange)="selectTenant($event.value)">
            <mat-option *ngIf="auth.isSuperUser()" [value]="null">All tenants</mat-option>
            <mat-option *ngFor="let tenant of tenants()" [value]="tenant.id">{{ tenant.name }}</mat-option>
          </mat-select>
        </mat-form-field>

        <button class="pf-icon-btn" (click)="toggleTheme()" [attr.aria-label]="theme.isDark ? 'Switch to light theme' : 'Switch to dark theme'">
          <mat-icon>{{ theme.isDark ? 'light_mode' : 'dark_mode' }}</mat-icon>
        </button>
        <button class="pf-icon-btn pf-notif" aria-label="Notifications">
          <mat-icon>notifications</mat-icon>
          <span class="pf-notif__dot"></span>
        </button>

        <div class="pf-user-chip">
          <div class="pf-avatar">{{ initials() }}</div>
          <div class="pf-user-chip__meta">
            <span class="pf-user-chip__name">{{ userEmail() }}</span>
            <span class="pf-user-chip__role">{{ roleLabel() }}</span>
          </div>
          <button mat-icon-button class="pf-user-chip__logout" (click)="logout()" aria-label="Sign out">
            <mat-icon>logout</mat-icon>
          </button>
        </div>
      </div>
    </header>
  `
})
export class HeaderComponent {
  @Output() toggleMenu = new EventEmitter<void>();

  readonly auth = inject(AuthService);
  readonly theme = inject(ThemeService);
  private readonly router = inject(Router);

  readonly tenants = computed(() => this.auth.currentUser()?.tenants ?? []);
  readonly activeTenantId = computed(() => this.auth.activeTenantIdSignal());
  readonly userEmail = computed(() => this.auth.currentUser()?.email ?? '');
  readonly isSuperUser = computed(() => this.auth.isSuperUser());
  readonly canSwitchTenant = computed(() => this.tenants().length > 1 || this.isSuperUser());
  readonly initials = computed(() => {
    const email = this.auth.currentUser()?.email ?? '?';
    return email.charAt(0).toUpperCase();
  });
  readonly roleLabel = computed(() => {
    const tenant = this.auth.activeTenant();
    if (tenant) {
      return tenant.name;
    }
    return this.isSuperUser() ? 'Super admin' : 'No workspace';
  });

  toggleTheme(): void {
    this.theme.toggle();
  }

  selectTenant(tenantId: string | null): void {
    this.auth.selectTenant(tenantId);
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}