import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { of, tap } from 'rxjs';
import { MemoryCacheService } from '../services/local/memory-cache-service';

export const memoryCacheInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith('/api/')) return next(request);
  const cache = inject(MemoryCacheService);

  // Me always verifies the session; authentication responses must never be cached.
  if (request.url.startsWith('/api/auth/') && !request.url.endsWith('/signup'))
    return next(request);

  if (request.method === 'GET') {
    const key = request.urlWithParams;
    const cached = cache.get<HttpResponse<unknown>>(key);
    if (cached) return of(cached);

    const version = cache.version;
    return next(request).pipe(
      tap((event) => {
        if (event instanceof HttpResponse && cache.version === version) cache.set(key, event);
      }),
    );
  }

  return next(request).pipe(
    tap((event) => {
      if (event instanceof HttpResponse) cache.clear();
    }),
  );
};
