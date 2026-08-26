import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { Observable, finalize } from 'rxjs';
import { ProviderCredentialScope, SupportedProvider } from '../../../models/provider-credential.model';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-provider-form',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule, MatSlideToggleModule],
  template: `
    <div class="pf-page pf-form">
      <div class="pf-page-header">
        <div>
          <h1 class="pf-title">{{ isEditing ? 'Edit Credential' : 'New Credential' }}</h1>
          <p class="pf-subtitle">Per-tenant API keys — replacing appsettings. Leave secret empty to keep existing.</p>
        </div>
      </div>

      <div class="pf-alert" *ngIf="error">{{ error }}</div>
      <div class="pf-alert pf-alert--success" *ngIf="success">{{ success }}</div>

      <mat-card class="pf-card pf-form__card">
        <div class="pf-form__title">
          <div class="pf-feature-icon"><mat-icon>key</mat-icon></div>
          <div><h2>Provider credential</h2><p>Choose provider, scope and fill tenant-specific secrets.</p></div>
        </div>

        <mat-form-field appearance="outline" *ngIf="!isEditing">
          <mat-label>Provider Key</mat-label>
          <mat-select [(ngModel)]="providerKey" name="providerKey" (selectionChange)="onProviderChange()">
            <mat-option *ngFor="let p of supported" [value]="p.key">{{ p.label }} ({{ p.key }})</mat-option>
          </mat-select>
          <mat-hint>Or type custom key below</mat-hint>
        </mat-form-field>

        <mat-form-field appearance="outline" *ngIf="!isEditing">
          <mat-label>Provider Key (custom)</mat-label>
          <input matInput [(ngModel)]="providerKey" name="customKey" placeholder="FACEBOOK, openai, dalle ..." />
        </mat-form-field>

        <div class="pf-form-row">
          <mat-form-field appearance="outline">
            <mat-label>Scope</mat-label>
            <mat-select [(ngModel)]="scope" name="scope">
              <mat-option [value]="0">Social</mat-option>
              <mat-option [value]="1">AI Text</mat-option>
              <mat-option [value]="2">AI Image</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Display Name</mat-label>
            <input matInput [(ngModel)]="displayName" name="displayName" placeholder="My Facebook App" />
          </mat-form-field>
        </div>

        <mat-form-field appearance="outline">
          <mat-label>Description (optional)</mat-label>
          <input matInput [(ngModel)]="description" name="description" placeholder="Production Facebook app for Tenant X" />
        </mat-form-field>

        <!-- Facebook helper fields -->
        <ng-container *ngIf="isFacebook">
          <h3 class="pf-section-title">Facebook settings — stored in Settings JSON</h3>
          <div class="pf-form-row">
            <mat-form-field appearance="outline">
              <mat-label>App ID</mat-label>
              <input matInput [(ngModel)]="fbAppId" name="fbAppId" placeholder="1234567890" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Default Page ID</mat-label>
              <input matInput [(ngModel)]="fbPageId" name="fbPageId" placeholder="104..." />
            </mat-form-field>
          </div>
          <div class="pf-form-row">
            <mat-form-field appearance="outline">
              <mat-label>Redirect URI</mat-label>
              <input matInput [(ngModel)]="fbRedirectUri" name="fbRedirectUri" placeholder="https://your-app/callback" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>API Version</mat-label>
              <input matInput [(ngModel)]="fbApiVersion" name="fbApiVersion" placeholder="v26.0" />
            </mat-form-field>
          </div>
          <mat-slide-toggle [(ngModel)]="fbAppSecretProof" name="fbAppSecretProof">Enable AppSecret Proof</mat-slide-toggle>
        </ng-container>

        <mat-form-field appearance="outline">
          <mat-label>Secret / API Key {{ isEditing ? '(leave empty to keep)' : '' }}</mat-label>
          <input matInput type="password" [(ngModel)]="secretValue" name="secretValue" placeholder="{{ isFacebook ? 'App Secret' : 'sk-...' }}" />
          <mat-hint *ngIf="isEditing">Current: {{ maskedSecret || 'none' }}</mat-hint>
        </mat-form-field>

        <mat-form-field appearance="outline" *ngIf="!isFacebook">
          <mat-label>Settings JSON (optional)</mat-label>
          <textarea matInput rows="4" [(ngModel)]="settingsJson" name="settingsJson" placeholder='{"example": "value"}'></textarea>
          <mat-hint>Raw JSON stored as SettingsJson — visible in list masked. For Facebook, fields above auto-build this JSON.</mat-hint>
        </mat-form-field>

        <mat-form-field appearance="outline" *ngIf="isFacebook">
          <mat-label>Advanced Settings JSON (auto-generated)</mat-label>
          <textarea matInput rows="3" [(ngModel)]="settingsJson" name="settingsJsonFb" placeholder="Auto-generated"></textarea>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>KeyVault Reference (optional)</mat-label>
          <input matInput [(ngModel)]="keyVaultReference" name="keyVaultReference" placeholder="vault://..." />
          <mat-hint>If you use Azure Key Vault, put reference here and leave Secret empty.</mat-hint>
        </mat-form-field>

        <mat-slide-toggle [(ngModel)]="isEnabled" name="isEnabled">Enabled</mat-slide-toggle>

        <div class="pf-hint" *ngIf="jsonError" style="color:#dc2626; font-size:.8rem; margin-top:.5rem;">{{ jsonError }}</div>
      </mat-card>

      <mat-card class="pf-card pf-form-actions">
        <button mat-button (click)="cancel()">Cancel</button>
        <button mat-flat-button class="pf-btn-primary" (click)="save()" [disabled]="saving || !providerKey.trim() || !displayName.trim()">
          <mat-icon>{{ saving ? 'hourglass_top' : 'save' }}</mat-icon>
          {{ isEditing ? 'Update' : 'Create' }}
        </button>
      </mat-card>
    </div>
  `,
  styles: [`.pf-section-title{margin:1rem 0 .5rem; font-size:.9rem; color:#374151; font-weight:600;}`]
})
export class ProviderFormComponent implements OnInit {
  isEditing = false;
  credentialId = '';
  providerKey = 'FACEBOOK';
  scope: ProviderCredentialScope = ProviderCredentialScope.Social;
  displayName = '';
  description = '';
  keyVaultReference = '';
  secretValue = '';
  settingsJson = '';
  maskedSecret = '';
  isEnabled = true;
  saving = false;
  error: string | null = null;
  success: string | null = null;
  jsonError: string | null = null;

  supported: SupportedProvider[] = [];

  // facebook helper
  fbAppId = '';
  fbPageId = '';
  fbRedirectUri = '';
  fbApiVersion = 'v26.0';
  fbAppSecretProof = false;

  get isFacebook(): boolean { return this.providerKey?.toUpperCase() === 'FACEBOOK'; }

  constructor(private route: ActivatedRoute, private router: Router, private api: ApiService) {
    const id = this.route.snapshot.paramMap.get('id');
    this.isEditing = !!id && id !== 'new';
    this.credentialId = id && id !== 'new' ? id : '';
  }

  ngOnInit(): void {
    this.api.getSupportedProviders().subscribe({
      next: (list) => this.supported = list,
      error: () => this.supported = []
    });

    if (this.isEditing) {
      this.api.getProviderCredential(this.credentialId).subscribe({
        next: (c) => {
          this.providerKey = c.providerKey;
          this.scope = c.scope;
          this.displayName = c.displayName;
          this.description = c.description ?? '';
          this.keyVaultReference = c.keyVaultReference ?? '';
          this.settingsJson = c.settingsJson ?? '';
          this.maskedSecret = c.maskedSecret ?? '';
          this.isEnabled = c.isEnabled;
          this.tryParseFacebookSettings();
        },
        error: () => this.error = 'Unable to load credential.'
      });
    } else {
      this.displayName = 'Facebook';
    }
  }

  onProviderChange(): void {
    if (this.providerKey.toUpperCase() === 'FACEBOOK') {
      this.scope = ProviderCredentialScope.Social;
      if (!this.displayName || this.displayName === 'Facebook') this.displayName = 'Facebook';
    } else if (['openai','anthropic','google-gemini','microsoft-foundry'].includes(this.providerKey)) {
      this.scope = ProviderCredentialScope.AiText;
    } else if (['dalle','stable-diffusion'].includes(this.providerKey)) {
      this.scope = ProviderCredentialScope.AiImage;
    }
    this.tryParseFacebookSettings();
  }

  private tryParseFacebookSettings(): void {
    if (!this.isFacebook || !this.settingsJson) return;
    try {
      const parsed = JSON.parse(this.settingsJson);
      this.fbAppId = parsed.appId ?? parsed.AppId ?? '';
      this.fbPageId = parsed.defaultPageId ?? parsed.DefaultPageId ?? '';
      this.fbRedirectUri = parsed.redirectUri ?? parsed.RedirectUri ?? '';
      this.fbApiVersion = parsed.apiVersion ?? parsed.ApiVersion ?? 'v26.0';
      this.fbAppSecretProof = parsed.enableAppSecretProof ?? parsed.EnableAppSecretProof ?? false;
      // if secret also in json, keep it? but secret field separate so ignore
    } catch { /* ignore */ }
  }

  private buildFacebookSettingsJson(): void {
    if (!this.isFacebook) return;
    const obj: any = {};
    if (this.fbAppId) obj.appId = this.fbAppId;
    if (this.fbPageId) obj.defaultPageId = this.fbPageId;
    if (this.fbRedirectUri) obj.redirectUri = this.fbRedirectUri;
    if (this.fbApiVersion) obj.apiVersion = this.fbApiVersion;
    obj.enableAppSecretProof = this.fbAppSecretProof;
    // merge with existing settingsJson extra keys? keep simple overwrite
    // if user edited raw JSON manually, respect if it already contains other keys, parse merge
    if (this.settingsJson) {
      try {
        const existing = JSON.parse(this.settingsJson);
        // keep keys not in helper unless helper overrides
        const merged = { ...existing, ...obj };
        this.settingsJson = JSON.stringify(merged);
        return;
      } catch {}
    }
    this.settingsJson = JSON.stringify(obj);
  }

  cancel(): void { this.router.navigate(['/provider-credentials']); }

  save(): void {
    if (this.saving || !this.providerKey.trim() || !this.displayName.trim()) return;

    this.jsonError = null;
    if (this.isFacebook) this.buildFacebookSettingsJson();

    if (this.settingsJson) {
      try { JSON.parse(this.settingsJson); } catch (e:any) {
        this.jsonError = 'Settings JSON is not valid: ' + e.message;
        return;
      }
    }

    this.saving = true;
    this.error = null;
    this.success = null;

    const payloadBase = {
      displayName: this.displayName.trim(),
      description: this.description?.trim() || undefined,
      keyVaultReference: this.keyVaultReference?.trim() || undefined,
      secretValue: this.secretValue?.trim() || undefined,
      settingsJson: this.settingsJson?.trim() || undefined,
      isEnabled: this.isEnabled
    };

    let request: Observable<unknown>;
    if (this.isEditing) {
      // secretValue undefined means keep; if empty string we send undefined to keep
      const updatePayload = { ...payloadBase, secretValue: this.secretValue ? this.secretValue : undefined };
      request = this.api.updateProviderCredential(this.credentialId, updatePayload as any);
    } else {
      request = this.api.createProviderCredential({
        providerKey: this.providerKey.trim(),
        scope: this.scope,
        displayName: this.displayName.trim(),
        description: this.description?.trim() || undefined,
        keyVaultReference: this.keyVaultReference?.trim() || undefined,
        secretValue: this.secretValue?.trim() || undefined,
        settingsJson: this.settingsJson?.trim() || undefined,
        isEnabled: this.isEnabled
      });
    }

    request.pipe(finalize(() => this.saving = false)).subscribe({
      next: () => this.router.navigate(['/provider-credentials']),
      error: (err) => {
        const detail = err?.error?.errors ? JSON.stringify(err.error.errors) : err?.error?.detail ?? err?.message;
        this.error = 'Unable to save credential. ' + (detail ?? '');
      }
    });
  }
}
