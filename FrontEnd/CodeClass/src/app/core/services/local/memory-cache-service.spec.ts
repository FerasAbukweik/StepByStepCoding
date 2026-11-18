import { afterEach, describe, expect, it, vi } from 'vitest';
import { MemoryCacheService } from './memory-cache-service';

describe('MemoryCacheService', () => {
  afterEach(() => vi.useRealTimers());

  it('stores data and expires it after the specified duration', () => {
    vi.useFakeTimers();
    const cache = new MemoryCacheService();
    expect(cache.get('file')).toBeUndefined();
    cache.set('file', { code: 'hello' }, 1000);
    expect(cache.get('file')).toEqual({ code: 'hello' });
    vi.advanceTimersByTime(1000);
    expect(cache.get('file')).toBeUndefined();
  });

  it('clears matching prefixes and supports a full reset', () => {
    const cache = new MemoryCacheService();
    cache.set('/api/files/1', 'file');
    cache.set('/api/courses', 'course');
    cache.clear('/api/files');
    expect(cache.get('/api/files/1')).toBeUndefined();
    expect(cache.get('/api/courses')).toBe('course');
    cache.clear();
    expect(cache.get('/api/courses')).toBeUndefined();
  });
});
