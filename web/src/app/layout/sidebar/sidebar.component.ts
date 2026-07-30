import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatListModule, MatIconModule],
  template: `
    <mat-nav-list>
      <mat-list-item routerLink="/posts" routerLinkActive="active-link">
        <mat-icon matListItemIcon>article</mat-icon>
        <span matListItemTitle>Posts</span>
      </mat-list-item>
      <mat-list-item routerLink="/campaigns" routerLinkActive="active-link">
        <mat-icon matListItemIcon>campaign</mat-icon>
        <span matListItemTitle>Campaigns</span>
      </mat-list-item>
      <mat-list-item routerLink="/scheduling" routerLinkActive="active-link">
        <mat-icon matListItemIcon>calendar_month</mat-icon>
        <span matListItemTitle>Scheduling</span>
      </mat-list-item>
      <mat-list-item routerLink="/ai" routerLinkActive="active-link">
        <mat-icon matListItemIcon>auto_awesome</mat-icon>
        <span matListItemTitle>AI Assist</span>
      </mat-list-item>
    </mat-nav-list>
  `,
  styles: [`
    .active-link {
      background: rgba(63, 81, 181, 0.1);
      border-left: 3px solid #3f51b5;
    }
    mat-nav-list {
      padding-top: 8px;
    }
  `]
})
export class SidebarComponent {}
