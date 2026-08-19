export interface Tenant {
  id: string;
  name: string;
  slug: string;
  isActive: boolean;
  createdAtUtc: string;
}

export interface CreateTenantRequest {
  name: string;
  slug: string;
}

export interface TenantUser {
  userId: string;
  email: string;
  joinedAtUtc: string;
}

export interface AddTenantUserRequest {
  tenantId: string;
  email: string;
  password: string;
}