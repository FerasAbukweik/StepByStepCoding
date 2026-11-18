import { ChangePasswordDTO } from '../../DTO/auth/change-password.dto';
import { apiError } from '../../utils/api-error';
import { MemoryCacheService } from '../local/memory-cache-service';
import { catchErrorToast } from '../../Utilities/catch-error-toast';
import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, finalize, shareReplay, switchMap, tap, throwError } from 'rxjs';
import { ToastService } from '../local/toast-service';
import { AuthApiService } from '../api/auth-api-service';
import { UserDTO } from '../../DTO/auth/user.dto';
import { LoginDTO } from '../../DTO/auth/login.dto';

export const USER_STORAGE_KEY = 'codeclass.user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(AuthApiService);
  private readonly cache = inject(MemoryCacheService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly currentUser = signal<UserDTO | null>(this.readSavedUser());
  private meRequest?: Observable<UserDTO>;
  private refreshRequest?: Observable<void>;
  private sessionVersion = 0;

  readonly passwordSaving = signal(false);
  readonly passwordError = signal('');

  readonly user = this.currentUser.asReadonly();

  login(request: LoginDTO) {
    return this.api.login(request).pipe(
      switchMap(() => this.me()),
      catchErrorToast('Could not sign in. Check your credentials and try again.', this.toast),
    );
  }

  // A saved profile makes the shell immediate; only Me verifies authentication.
  me(): Observable<UserDTO> {
    if (this.meRequest) return this.meRequest;
    const version = this.sessionVersion;

    this.meRequest = this.api.me().pipe(
      tap((user) => {
        if (version !== this.sessionVersion) return;
        if (this.currentUser()?.id !== user.id) this.cache.clear();
        this.currentUser.set(user);
        try {
          localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(user));
        } catch {}
      }),
      finalize(() => {
        this.meRequest = undefined;
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
    return this.meRequest;
  }

  refresh(): Observable<void> {
    if (this.refreshRequest) return this.refreshRequest;

    this.refreshRequest = this.api.refresh().pipe(
      catchError((error) => {
        this.clearSession();
        return throwError(() => error);
      }),
      finalize(() => {
        this.refreshRequest = undefined;
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
    return this.refreshRequest;
  }

  logout() {
    return this.api.logout().pipe(
      tap(() => {
        this.clearSession();
        void this.router.navigateByUrl('/login');
      }),
      catchErrorToast('Could not sign out. Please try again.', this.toast),
    );
  }

  changePassword(request: ChangePasswordDTO) {
    this.passwordSaving.set(true);
    this.passwordError.set('');
    return this.api.changePassword(request).pipe(
      tap(() => {
        this.clearSession();
        this.toast.success('Password changed. Sign in with your new password.');
        void this.router.navigateByUrl('/login');
      }),
      tap({ error: (error) => this.passwordError.set(apiError(error)) }),
      catchErrorToast('Could not change your password.', this.toast),
      finalize(() => this.passwordSaving.set(false)),
    );
  }

  clearSession(): void {
    this.cache.clear();
    this.sessionVersion++;
    this.currentUser.set(null);
    try {
      localStorage.removeItem(USER_STORAGE_KEY);
    } catch {}
  }

  hasRole(role: string): boolean {
    return this.currentUser()?.roles.includes(role) ?? false;
  }

  canTeach(): boolean {
    return this.hasRole('Instructor') || this.hasRole('Admin');
  }

  private readSavedUser(): UserDTO | null {
    try {
      const value: unknown = JSON.parse(localStorage.getItem(USER_STORAGE_KEY) ?? 'null');
      if (!value || typeof value !== 'object') return null;
      const user = value as Partial<UserDTO>;
      return typeof user.id === 'string' &&
        typeof user.fullName === 'string' &&
        typeof user.email === 'string' &&
        Array.isArray(user.roles) &&
        user.roles.every((role) => typeof role === 'string')
        ? (user as UserDTO)
        : null;
    } catch {
      return null;
    }
  }
}
