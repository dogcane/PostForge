import { Inject, Injectable } from '@angular/core';
import { DOCUMENT } from '@angular/common';

const THEME_KEY = 'pf-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  isDark: boolean;

  constructor(@Inject(DOCUMENT) private document: Document) {
    const stored = this.document.defaultView?.localStorage.getItem(THEME_KEY);
    this.isDark = stored === 'light' ? false : true;
    this.apply();
  }

  toggle(): void {
    this.isDark = !this.isDark;
    this.document.defaultView?.localStorage.setItem(THEME_KEY, this.isDark ? 'dark' : 'light');
    this.apply();
  }

  private apply(): void {
    this.document.documentElement.setAttribute('data-theme', this.isDark ? 'dark' : 'light');
  }
}
