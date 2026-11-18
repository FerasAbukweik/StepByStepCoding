import { Injectable } from '@angular/core';

interface CacheEntry {
  value: unknown;
  expiresAt: number;
}

@Injectable({ providedIn: 'root' })
export class MemoryCacheService {
  private readonly entries = new Map<string, CacheEntry>();
  private revision = 0;

  get version(): number {
    return this.revision;
  }

  get<T>(key: string): T | undefined {
    const cached = this.entries.get(key);
    if (!cached) return undefined;
    if (cached.expiresAt <= Date.now()) {
      this.entries.delete(key);
      return undefined;
    }
    return cached.value as T;
  }

  set<T>(key: string, value: T, duration = 30000): void {
    this.entries.set(key, { value, expiresAt: Date.now() + duration });
  }

  clear(prefix = ''): void {
    this.revision++;
    for (const key of this.entries.keys()) {
      if (key.startsWith(prefix)) this.entries.delete(key);
    }
  }
}
