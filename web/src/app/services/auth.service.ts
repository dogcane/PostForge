import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { CurrentUser, LoginRequest, LoginResult, RefreshTokenRequest } from '../models/user.model';
import { Tenant } from '../models/tenant.model';

const AUTH_KEY = 'pf-auth';
const TENANT_KEY = 'pf-tenant';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = '/api/v1/auth';

  private authState = signal<LoginResult | null>(null);
  private user = signal<CurrentUser | null>(null);
  private activeTenantId = signal<string | null>(null);

  readonly isAuthenticated = computed(() => !!this.authState());
  readonly isSuperUser = computed(() => !!this.user()?.isSuperUser);
  readonly currentToken = computed(() => this.authState()?.token ?? null);
  readonly currentUser = this.user.asReadonly();
  readonly activeTenantIdSignal = this.activeTenantId.asReadonly();
  readonly activeTenant = computed<Tenant | null>(() => {
    const id = this.activeTenantId();
    if (!id) {
      return null;
    }
    return this.user()?.tenants.find((t) => t.id === id) ?? null;
  });

  constructor(private http: HttpClient) {
    this.restore();
  }

  login(email: string, password: string): Observable<LoginResult> {
    const body: LoginRequest = { email, password };
    return this.http.post<LoginResult>(`${this.baseUrl}/login`, body).pipe(
      tap((result) => this.setSession(result))
    );
  }

  refresh(): Observable<LoginResult> {
    const current = this.authState();
    const token = current?.refreshToken;
    if (!token) {
      throw new Error('No refresh token available');
    }
    const body: RefreshTokenRequest = { refreshToken: token };
    return this.http.post<LoginResult>(`${this.baseUrl}/refresh`, body).pipe(
      tap((result) => this.setSession(result))
    );
  }

  loadCurrentUser(): Observable<CurrentUser> {
    return this.http.get<CurrentUser>(`${this.baseUrl}/me`).pipe(
      tap((user) => {
        this.user.set(user);
        const stored = this.activeTenantId();
        const stillValid = stored !== null && user.tenants.some((t) => t.id === stored);
        if (!stillValid) {
          this.activeTenantId.set(user.tenants.length > 0 ? user.tenants[0].id : null);
          this.persistTenant();
        }
      })
    );
  }

  selectTenant(tenantId: string | null): void {
    this.activeTenantId.set(tenantId);
    this.persistTenant();
  }

  logout(): void {
    const refreshToken = this.authState()?.refreshToken;
    if (refreshToken) {
      this.http.post(`${this.baseUrl}/logout`, { refreshToken }).subscribe({
        error: () => {}
      });
    }
    this.clearSession();
  }

  clearSession(): void {
    this.authState.set(null);
    this.user.set(null);
    this.activeTenantId.set(null);
    try {
      localStorage.removeItem(AUTH_KEY);
      localStorage.removeItem(TENANT_KEY);
    } catch {
      // ignore storage errors
    }
  }

  hasRefreshToken(): boolean {
    return !!this.authState()?.refreshToken;
  }

  private setSession(result: LoginResult): void {
    this.authState.set(result);
    try {
      localStorage.setItem(AUTH_KEY, JSON.stringify(result));
    } catch {
      // ignore storage errors
    }
  }

  private restore(): void {
    try {
      const raw = localStorage.getItem(AUTH_KEY);
      if (raw) {
        this.authState.set(JSON.parse(raw) as LoginResult);
      }
      this.activeTenantId.set(localStorage.getItem(TENANT_KEY));
    } catch {
      // ignore storage errors
    }
  }

  private persistTenant(): void {
    const id = this.activeTenantId();
    try {
      if (id) {
        localStorage.setItem(TENANT_KEY, id);
      } else {
        localStorage.removeItem(TENANT_KEY);
      }
    } catch {
      // ignore storage errors
    }
  }
}