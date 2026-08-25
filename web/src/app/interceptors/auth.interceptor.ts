import { HttpErrorResponse, HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;
let refreshSubject: BehaviorSubject<string | null> | null = null;

function isAuthUrl(url: string): boolean {
  return url.includes('/api/v1/auth/');
}

function isRefreshUrl(url: string): boolean {
  return url.includes('/api/v1/auth/refresh');
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.currentToken();
  const tenantId = auth.activeTenantIdSignal();
  const isAuthCall = isAuthUrl(req.url);

  let headers = req.headers;
  if (token) {
    headers = headers.set('Authorization', `Bearer ${token}`);
  }
  if (tenantId && !isAuthCall) {
    headers = headers.set('X-Tenant-Id', tenantId);
  }

  const request = req.clone({ headers });

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthCall || isRefreshUrl(req.url)) {
        if (error.status === 401 && isAuthCall && !isRefreshUrl(req.url)) {
          // login failure — do not attempt refresh
          return throwError(() => error);
        }
        if (error.status === 401 && isRefreshUrl(req.url)) {
          auth.clearSession();
          router.navigate(['/login']);
          return throwError(() => error);
        }
        return throwError(() => error);
      }

      if (!auth.hasRefreshToken()) {
        auth.clearSession();
        router.navigate(['/login']);
        return throwError(() => error);
      }

      return handleRefresh(req, next, auth, router);
    })
  );
};

function handleRefresh(req: HttpRequest<unknown>, next: HttpHandlerFn, auth: AuthService, router: Router): Observable<import('@angular/common/http').HttpEvent<unknown>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshSubject = new BehaviorSubject<string | null>(null);

    auth.refresh().subscribe({
      next: (result) => {
        isRefreshing = false;
        refreshSubject!.next(result.token);
        refreshSubject!.complete();
        refreshSubject = null;
      },
      error: () => {
        isRefreshing = false;
        refreshSubject!.error(new Error('refresh failed'));
        refreshSubject = null;
        auth.clearSession();
        router.navigate(['/login']);
      }
    });
  }

  return refreshSubject!.pipe(
    filter((t) => t !== null),
    take(1),
    switchMap((newToken) => {
      const tenantId = auth.activeTenantIdSignal();
      let headers = req.headers.set('Authorization', `Bearer ${newToken}`);
      if (tenantId && !isAuthUrl(req.url)) {
        headers = headers.set('X-Tenant-Id', tenantId);
      }
      return next(req.clone({ headers }));
    }),
    catchError((err) => throwError(() => err))
  );
}