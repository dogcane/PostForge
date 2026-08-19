import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from '../header/header.component';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, SidebarComponent],
  template: `
    <div class="pf-shell">
      <div class="pf-backdrop" [class.show]="sidebarOpen" (click)="sidebarOpen = false"></div>
      <app-sidebar [open]="sidebarOpen" (close)="sidebarOpen = false"></app-sidebar>

      <div class="pf-main">
        <app-header (toggleMenu)="toggleSidebar()"></app-header>
        <div class="pf-glow pf-glow--tr"></div>
        <div class="pf-glow pf-glow--bl"></div>
        <main class="pf-content">
          <router-outlet />
        </main>
      </div>
    </div>
  `
})
export class MainLayoutComponent implements OnInit {
  sidebarOpen = false;

  constructor(private auth: AuthService) {}

  ngOnInit(): void {
    this.auth.loadCurrentUser().subscribe({
      error: () => {
        // The interceptor handles 401 by clearing the session and redirecting to login.
      }
    });
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }
}
