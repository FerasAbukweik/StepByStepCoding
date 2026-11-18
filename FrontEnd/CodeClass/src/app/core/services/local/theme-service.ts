import { DOCUMENT } from '@angular/common';
import { Injectable, inject, signal } from '@angular/core';

export type Theme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly current = signal<Theme>(
    this.document.documentElement.classList.contains('dark') ? 'dark' : 'light',
  );

  readonly theme = this.current.asReadonly();

  setTheme(theme: Theme): void {
    this.current.set(theme);
    this.document.documentElement.classList.toggle('dark', theme === 'dark');
    this.document.documentElement.style.colorScheme = theme;
    try {
      localStorage.setItem('codeclass.theme', theme);
    } catch {}
  }

  toggle(): void {
    this.setTheme(this.current() === 'dark' ? 'light' : 'dark');
  }
}
