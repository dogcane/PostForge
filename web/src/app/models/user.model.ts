import { Tenant } from './tenant.model';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResult {
  token: string;
  expiresAtUtc: string;
  refreshToken: string;
  refreshExpiresAtUtc: string;
  userId: string;
  email: string;
  isSuperUser: boolean;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface CurrentUser {
  userId: string;
  email: string;
  isSuperUser: boolean;
  tenants: Tenant[];
}