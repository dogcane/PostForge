export enum ProviderCredentialScope {
  Social = 0,
  AiText = 1,
  AiImage = 2
}

export interface ProviderCredential {
  id: string;
  providerKey: string;
  scope: ProviderCredentialScope;
  displayName: string;
  description?: string;
  keyVaultReference?: string;
  maskedSecret?: string;
  hasSecret: boolean;
  settingsJson?: string;
  isEnabled: boolean;
  isValidated: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateProviderCredentialRequest {
  providerKey: string;
  scope: ProviderCredentialScope;
  displayName: string;
  description?: string;
  keyVaultReference?: string;
  secretValue?: string;
  settingsJson?: string;
  isEnabled: boolean;
}

export interface UpdateProviderCredentialRequest {
  displayName: string;
  description?: string;
  keyVaultReference?: string;
  secretValue?: string;
  settingsJson?: string;
  isEnabled: boolean;
}

export interface SupportedProvider {
  key: string;
  scope: ProviderCredentialScope;
  label: string;
  description: string;
}

export function scopeLabel(scope: ProviderCredentialScope): string {
  switch (scope) {
    case ProviderCredentialScope.Social: return 'Social';
    case ProviderCredentialScope.AiText: return 'AI Text';
    case ProviderCredentialScope.AiImage: return 'AI Image';
    default: return String(scope);
  }
}

export function scopeClass(scope: ProviderCredentialScope): string {
  switch (scope) {
    case ProviderCredentialScope.Social: return 'pf-scope--social';
    case ProviderCredentialScope.AiText: return 'pf-scope--aitext';
    case ProviderCredentialScope.AiImage: return 'pf-scope--aiimage';
    default: return '';
  }
}

export function scopeIcon(scope: ProviderCredentialScope): string {
  switch (scope) {
    case ProviderCredentialScope.Social: return 'share';
    case ProviderCredentialScope.AiText: return 'chat';
    case ProviderCredentialScope.AiImage: return 'image';
    default: return 'key';
  }
}
