import { Component, effect, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import {
  ProviderCredential,
  scopeClass,
  scopeIcon,
  scopeLabel
} from '../../../models/provider-credential.model';
import { ApiService } from '../../../services/api.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-provider-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, MatIconModule, MatChipsModule, MatSlideToggleModule],
  template: `
    <div class="pf-page">
      <div class="pf-page-header">
        <div>
          <h1 class="pf-title">Provider Credentials</h1>
          <p class="pf-subtitle">Manage per-tenant API keys for social and AI providers</p>
        </div>
        <a mat-flat-button class="pf-btn-primary" routerLink="/provider-credentials/new">
          <mat-icon>add</mat-icon>
          New Credential
        </a>
      </div>

      <div class="pf-alert" *ngIf="error">{{ error }}</div>

      <ng-container *ngIf="!loading; else loadingState">
        <ng-container *ngIf="credentials.length; else empty">
          <div class="pf-grid pf-grid--credentials">
            <article class="pf-card pf-card--hover credential-card" *ngFor="let c of credentials">
              <div class="credential-card__head">
                <span class="pf-scope" [class]="scopeClass(c.scope)">
                  <mat-icon>{{ scopeIcon(c.scope) }}</mat-icon>
                  {{ scopeLabel(c.scope) }}
                </span>
                <span class="pf-provider-key">{{ c.providerKey }}</span>
                <span class="spacer"></span>
                <span class="pf-badge" [class.pf-badge--success]="c.isValidated" [class.pf-badge--warn]="!c.isValidated">
                  <mat-icon>{{ c.isValidated ? 'verified' : 'hourglass_empty' }}</mat-icon>
                  {{ c.isValidated ? 'Validated' : 'Not validated' }}
                </span>
              </div>
              <h3 class="credential-card__name">{{ c.displayName }}</h3>
              <p class="credential-card__desc" *ngIf="c.description">{{ c.description }}</p>
              <div class="credential-card__meta">
                <span>
                  <mat-icon>key</mat-icon>
                  {{ c.hasSecret ? (c.maskedSecret ?? '••••') : 'No secret' }}
                </span>
                <span class="pf-dot" [class.enabled]="c.isEnabled" [class.disabled]="!c.isEnabled">
                  {{ c.isEnabled ? 'Enabled' : 'Disabled' }}
                </span>
              </div>
              <div class="credential-card__settings" *ngIf="c.settingsJson">
                <code class="pf-code">{{ c.settingsJson | slice:0:120 }}{{ c.settingsJson.length > 120 ? '…' : '' }}</code>
              </div>
              <div class="credential-card__actions">
                <a mat-stroked-button [routerLink]="['/provider-credentials', c.id]">Edit</a>
                <button mat-stroked-button (click)="validate(c)" [disabled]="c.isValidated">
                  <mat-icon>verified</mat-icon>
                  Validate
                </button>
                <button mat-icon-button (click)="deleteCredential(c)" aria-label="Delete credential">
                  <mat-icon>delete_outline</mat-icon>
                </button>
              </div>
            </article>
          </div>
        </ng-container>
      </ng-container>

      <ng-template #empty>
        <div class="pf-empty">
          <mat-icon>key</mat-icon>
          <h3>No credentials yet</h3>
          <p>Add your first provider credential. Facebook and AI providers now live per tenant, not in appsettings.</p>
          <a mat-flat-button class="pf-btn-primary" routerLink="/provider-credentials/new">
            <mat-icon>add</mat-icon>
            Create credential
          </a>
        </div>
      </ng-template>

      <ng-template #loadingState>
        <div class="pf-loading"><mat-icon>autorenew</mat-icon> Loading credentials...</div>
      </ng-template>
    </div>
  `,
  styles: [`
    .pf-grid--credentials { grid-template-columns: repeat(auto-fill, minmax(380px, 1fr)); }
    .credential-card { padding: 1rem 1.25rem; }
    .credential-card__head { display:flex; align-items:center; gap:.5rem; margin-bottom:.5rem; }
    .pf-provider-key { font-weight:700; font-size:.85rem; letter-spacing:.05em; background:#f3f4f6; padding:2px 6px; border-radius:4px; }
    .pf-scope { display:inline-flex; align-items:center; gap:4px; font-size:.75rem; padding:2px 8px; border-radius:999px; background:#eef2ff; color:#4338ca; }
    .pf-scope--aitext { background:#ecfdf5; color:#065f46; }
    .pf-scope--aiimage { background:#fef3c7; color:#92400e; }
    .pf-badge--success { background:#ecfdf5; color:#065f46; padding:2px 6px; border-radius:999px; font-size:.75rem; }
    .pf-badge--warn { background:#fff7ed; color:#9a3412; padding:2px 6px; border-radius:999px; font-size:.75rem; }
    .credential-card__name { margin:0 0 .25rem; font-size:1.1rem; }
    .credential-card__desc { margin:0 0 .5rem; color:#6b7280; font-size:.85rem; }
    .credential-card__meta { display:flex; gap:1rem; align-items:center; font-size:.8rem; color:#6b7280; margin-bottom:.5rem; }
    .pf-dot::before { content:'●'; margin-right:4px; }
    .pf-dot.enabled { color:#16a34a; }
    .pf-dot.disabled { color:#dc2626; }
    .pf-code { font-size:.7rem; background:#f9fafb; padding:4px 6px; border-radius:4px; display:block; overflow:hidden; word-break:break-all; }
    .credential-card__actions { display:flex; gap:.5rem; margin-top:.75rem; }
  `]
})
export class ProviderListComponent {
  credentials: ProviderCredential[] = [];
  loading = false;
  error: string | null = null;

  private readonly auth = inject(AuthService);

  constructor(private api: ApiService) {
    effect(() => {
      this.auth.activeTenantIdSignal();
      this.load();
    });
  }

  private load(): void {
    this.loading = true;
    this.error = null;
    this.api.getProviderCredentials().subscribe({
      next: (list) => {
        this.credentials = list;
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load provider credentials.';
        this.loading = false;
      }
    });
  }

  deleteCredential(c: ProviderCredential): void {
    if (!confirm(`Delete credential "${c.displayName}" (${c.providerKey})?`)) return;
    this.api.deleteProviderCredential(c.id).subscribe({
      next: () => this.load(),
      error: () => this.error = 'Unable to delete credential.'
    });
  }

  validate(c: ProviderCredential): void {
    this.api.validateProviderCredential(c.id).subscribe({
      next: () => this.load(),
      error: () => this.error = 'Unable to validate credential.'
    });
  }

  readonly scopeLabel = scopeLabel;
  readonly scopeClass = scopeClass;
  readonly scopeIcon = scopeIcon;
}
