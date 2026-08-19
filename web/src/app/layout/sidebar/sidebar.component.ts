import { Component, EventEmitter, Input, Output, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, MatIconModule],
  template: `
    <aside class="pf-sidebar" [class.open]="open">
      <div class="pf-sidebar__glow"></div>
      <div class="pf-sidebar__inner">
        <div class="pf-brand">
          <div class="pf-logo">
            <mat-icon>auto_fix_high</mat-icon>
          </div>
          <div class="pf-brand__text">
            <span class="pf-brand__name">PostForge</span>
            <span class="pf-brand__sub">Content Studio</span>
          </div>
        </div>

        <a class="pf-cta" routerLink="/posts/new" (click)="close.emit()">
          <mat-icon>add</mat-icon>
          <span>New Post</span>
        </a>

        <p class="pf-sidebar__label">Workspace</p>
        <nav class="pf-nav">
          <a class="pf-nav__item" routerLink="/posts" routerLinkActive="active" (click)="close.emit()">
            <mat-icon>article</mat-icon>
            <span>Posts</span>
          </a>
          <a class="pf-nav__item" routerLink="/campaigns" routerLinkActive="active" (click)="close.emit()">
            <mat-icon>campaign</mat-icon>
            <span>Campaigns</span>
          </a>
          <a class="pf-nav__item" routerLink="/scheduling" routerLinkActive="active" (click)="close.emit()">
            <mat-icon>calendar_month</mat-icon>
            <span>Editorial Calendar</span>
          </a>
          <a class="pf-nav__item" routerLink="/tenants" routerLinkActive="active" (click)="close.emit()" *ngIf="isSuperUser()">
            <mat-icon>domain</mat-icon>
            <span>Tenants</span>
          </a>
        </nav>

        <p class="pf-sidebar__label">Tools</p>
        <nav class="pf-nav">
          <a class="pf-nav__item" routerLink="/ai" routerLinkActive="active" (click)="close.emit()">
            <mat-icon>auto_awesome</mat-icon>
            <span>AI Assist</span>
          </a>
        </nav>

        <div class="pf-sidebar__footer">
          <div class="pf-user">
            <div class="pf-avatar">{{ initials() }}</div>
            <div class="pf-user__meta">
              <span class="pf-user__name">{{ userEmail() }}</span>
              <span class="pf-user__role">{{ tenantLabel() }}</span>
            </div>
            <button class="pf-user__gear" (click)="logout()" aria-label="Sign out">
              <mat-icon>logout</mat-icon>
            </button>
          </div>
        </div>
      </div>
    </aside>
  `
})
export class SidebarComponent {
  @Input() open = false;
  @Output() close = new EventEmitter<void>();

  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly isSuperUser = computed(() => this.auth.isSuperUser());
  readonly userEmail = computed(() => this.auth.currentUser()?.email ?? 'Account');
  readonly initials = computed(() => (this.auth.currentUser()?.email ?? 'A').charAt(0).toUpperCase());
  readonly tenantLabel = computed(() => {
    const tenant = this.auth.activeTenant();
    if (tenant) {
      return tenant.name;
    }
    return this.auth.isSuperUser() ? 'Super admin' : 'No workspace';
  });

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}