import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AddTenantUserRequest,
  CreateTenantRequest,
  Tenant,
  TenantUser
} from '../models/tenant.model';

@Injectable({ providedIn: 'root' })
export class TenantService {
  private baseUrl = '/api/v1/tenants';

  constructor(private http: HttpClient) {}

  getTenants(): Observable<Tenant[]> {
    return this.http.get<Tenant[]>(this.baseUrl);
  }

  getTenant(id: string): Observable<Tenant> {
    return this.http.get<Tenant>(`${this.baseUrl}/${id}`);
  }

  createTenant(request: CreateTenantRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  getUsers(tenantId: string): Observable<TenantUser[]> {
    return this.http.get<TenantUser[]>(`${this.baseUrl}/${tenantId}/users`);
  }

  addUser(request: AddTenantUserRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/${request.tenantId}/users`, request);
  }

  removeUser(tenantId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${tenantId}/users/${userId}`);
  }
}