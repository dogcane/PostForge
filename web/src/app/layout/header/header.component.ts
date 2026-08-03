import { Component, EventEmitter, Output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [MatIconModule],
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
        <button class="pf-icon-btn" (click)="toggleTheme()" [attr.aria-label]="theme.isDark ? 'Switch to light theme' : 'Switch to dark theme'">
          <mat-icon>{{ theme.isDark ? 'light_mode' : 'dark_mode' }}</mat-icon>
        </button>
        <button class="pf-icon-btn pf-notif" aria-label="Notifications">
          <mat-icon>notifications</mat-icon>
          <span class="pf-notif__dot"></span>
        </button>
        <div class="pf-avatar">A</div>
      </div>
    </header>
  `
})
export class HeaderComponent {
  @Output() toggleMenu = new EventEmitter<void>();

  constructor(readonly theme: ThemeService) {}

  toggleTheme(): void {
    this.theme.toggle();
  }
}
