import { Tenant } from './tenant.model';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResult {
  token: string;
  expiresAtUtc: string;
  userId: string;
  email: string;
  isSuperUser: boolean;
}

export interface CurrentUser {
  userId: string;
  email: string;
  isSuperUser: boolean;
  tenants: Tenant[];
}