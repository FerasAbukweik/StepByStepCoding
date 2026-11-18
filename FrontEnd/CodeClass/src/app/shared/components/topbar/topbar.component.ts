import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ThemeService } from '../../../core/services/local/theme-service';
import { AuthService } from '../../../core/services/data/auth-service';
import { IconComponent } from '../icon/icon.component';
import { NotificationsComponent } from '../notifications/notifications.component';

@Component({
  selector: 'app-topbar',
  imports: [RouterLink, IconComponent, NotificationsComponent],
  templateUrl: './topbar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TopbarComponent {
  readonly menu = output<void>();
  protected readonly theme = inject(ThemeService);
  protected readonly auth = inject(AuthService);
  private readonly destroy = inject(DestroyRef);
  protected readonly signingOut = signal(false);
  protected readonly initials = computed(() =>
    (this.auth.user()?.fullName ?? '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((word) => word[0])
      .join('')
      .toUpperCase(),
  );

  logout(): void {
    if (this.signingOut()) return;
    this.signingOut.set(true);
    this.auth
      .logout()
      .pipe(
        finalize(() => this.signingOut.set(false)),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe();
  }
}
