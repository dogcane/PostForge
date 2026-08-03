import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatIconModule],
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
            <div class="pf-avatar">A</div>
            <div class="pf-user__meta">
              <span class="pf-user__name">Admin</span>
              <span class="pf-user__role">Workspace</span>
            </div>
            <mat-icon class="pf-user__gear">settings</mat-icon>
          </div>
        </div>
      </div>
    </aside>
  `
})
export class SidebarComponent {
  @Input() open = false;
  @Output() close = new EventEmitter<void>();
}
