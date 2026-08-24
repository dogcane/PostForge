import { Component, EventEmitter, Output, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatMenuModule, MatButtonModule],
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
        <div class="pf-workspace" *ngIf="canSwitchTenant()">
          <button class="pf-workspace__btn" [matMenuTriggerFor]="workspaceMenu" aria-label="Switch workspace">
            <span class="pf-workspace__text">
              <span class="pf-workspace__eyebrow">Workspace</span>
              <span class="pf-workspace__name">{{ activeTenantName() }}</span>
            </span>
            <mat-icon class="pf-workspace__chevron">expand_more</mat-icon>
          </button>
          <mat-menu #workspaceMenu="matMenu" class="pf-workspace__menu">
            <button mat-menu-item *ngIf="auth.isSuperUser()" (click)="selectTenant(null)">
              <mat-icon>language</mat-icon>
              <span>All tenants</span>
              <mat-icon *ngIf="activeTenantId() === null" class="pf-workspace__check">check</mat-icon>
            </button>
            <button mat-menu-item *ngFor="let tenant of tenants()" (click)="selectTenant(tenant.id)">
              <span>{{ tenant.name }}</span>
              <mat-icon *ngIf="activeTenantId() === tenant.id" class="pf-workspace__check">check</mat-icon>
            </button>
          </mat-menu>
        </div>

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
  readonly activeTenantName = computed(() => this.auth.activeTenant()?.name ?? (this.isSuperUser() ? 'All tenants' : '—'));
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