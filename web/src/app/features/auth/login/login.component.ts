import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { finalize, switchMap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-login',
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
    <div class="pf-auth">
      <mat-card class="pf-auth__card">
        <div class="pf-brand pf-auth__brand">
          <div class="pf-logo">
            <mat-icon>auto_fix_high</mat-icon>
          </div>
          <div class="pf-brand__text">
            <span class="pf-brand__name">PostForge</span>
            <span class="pf-brand__sub">Content Studio</span>
          </div>
        </div>

        <h1 class="pf-auth__title">Welcome back</h1>
        <p class="pf-auth__subtitle">Sign in to manage your workspace and publishing plan.</p>

        <form (ngSubmit)="login()">
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" [(ngModel)]="email" name="email" autocomplete="email" placeholder="you@example.com" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Password</mat-label>
            <input matInput type="password" [(ngModel)]="password" name="password" autocomplete="current-password" placeholder="Your password" />
          </mat-form-field>

          <div class="pf-auth__error" *ngIf="error">{{ error }}</div>

          <button mat-flat-button type="submit" class="pf-btn-primary pf-auth__submit" [disabled]="loading">
            <mat-icon>{{ loading ? 'hourglass_top' : 'login' }}</mat-icon>
            {{ loading ? 'Signing in...' : 'Sign in' }}
          </button>
        </form>

        @if (isDemo) {
          <p class="pf-auth__hint">
            Demo mode — no backend required.<br />
            Sign in as <code>admin@postforge.dev</code> (super admin) or <code>demo@postforge.dev</code>. Any password works.
          </p>
        } @else {
          <p class="pf-auth__hint">
            Default local admin: <code>admin@postforge.dev</code> / <code>Admin!12345</code>
          </p>
        }
      </mat-card>
    </div>
  `
})
export class LoginComponent {
  readonly isDemo = environment.demoMode;
  email = '';
  password = '';
  loading = false;
  error: string | null = null;

  constructor(
    private auth: AuthService,
    private router: Router
  ) {}

  login(): void {
    if (this.loading) {
      return;
    }
    if (!this.email.trim() || !this.password) {
      this.error = 'Enter your email and password.';
      return;
    }

    this.loading = true;
    this.error = null;

    this.auth
      .login(this.email.trim(), this.password)
      .pipe(
        switchMap(() => this.auth.loadCurrentUser()),
        finalize(() => (this.loading = false))
      )
      .subscribe({
        next: () => this.router.navigate(['/posts']),
        error: (err) => {
          this.error =
            err.status === 401
              ? 'Invalid email or password.'
              : 'Unable to log in. Check your connection and try again.';
        }
      });
  }
}