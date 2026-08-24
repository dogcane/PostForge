import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { environment } from '../environments/environment';
import { routes } from './app.routes';
import { authInterceptor } from './interceptors/auth.interceptor';
import { demoInterceptor } from './demo/demo.interceptor';

const httpInterceptors = environment.demoMode
  ? [authInterceptor, demoInterceptor]
  : [authInterceptor];

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimations(),
    provideHttpClient(withInterceptors(httpInterceptors))
  ]
};
