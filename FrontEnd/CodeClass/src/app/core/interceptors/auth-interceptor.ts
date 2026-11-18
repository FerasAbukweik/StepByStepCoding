import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/data/auth-service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith('/api/')) return next(request);

  const auth = inject(AuthService);
  const router = inject(Router);
  const authenticated = request.clone({ withCredentials: true });
  const anonymous = /\/auth\/(login|refresh-token)$/.test(request.url);

  return next(authenticated).pipe(
    catchError((error) => {
      if (error.status !== 401 || anonymous) return throwError(() => error);

      // Concurrent failures share one refresh request; retry each original request once.
      return auth.refresh().pipe(
        catchError((refreshError) => {
          void router.navigate(['/login'], {
            queryParams: { expired: '1', returnUrl: router.url },
          });
          return throwError(() => refreshError);
        }),
        switchMap(() => next(authenticated)),
      );
    }),
  );
};
