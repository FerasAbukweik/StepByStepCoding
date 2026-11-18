import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NotificationDTO } from '../../../core/DTO/realtime/notification.dto';
import { SignalRService } from '../../../core/services/signalr/signalr-service';
import { AuthService } from '../../../core/services/data/auth-service';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-notifications',
  imports: [DatePipe, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="relative">
    <button
      class="icon-button relative"
      type="button"
      aria-label="Notifications"
      [attr.aria-expanded]="open()"
      (click)="open.update(toggle)"
    >
      <app-icon name="notifications" />
      @if (items().length) {
        <span class="absolute -top-1 -right-1 text-[9px] bg-accentBlue text-white rounded px-1">{{
          items().length
        }}</span>
      }
    </button>
    @if (open()) {
      <div
        class="absolute z-50 right-0 top-10 w-80 max-w-[85vw] bg-surface border border-borderPrimary rounded shadow-xl"
      >
        <div class="flex justify-between p-3 border-b border-borderSubtle">
          <span class="font-medium">Notifications</span
          ><button (click)="open.set(false)" aria-label="Close notifications">×</button>
        </div>
        <div class="max-h-80 overflow-y-auto">
          @for (item of items(); track item.id) {
            <article class="p-3 border-b border-borderSubtle">
              <h3 class="font-medium text-sm">{{ item.title }}</h3>
              <p class="text-xs text-textSecondary mt-1">{{ item.message }}</p>
              <p class="text-2xs text-textMuted mt-2">
                {{ item.createdAt | date: 'short' }}
              </p>
            </article>
          } @empty {
            <p class="p-5 text-sm text-textSecondary">You're all caught up.</p>
          }
        </div>
      </div>
    }
  </div>`,
})
export class NotificationsComponent {
  private readonly hub = inject(SignalRService);
  private readonly auth = inject(AuthService);
  protected readonly items = signal<NotificationDTO[]>([]);
  protected readonly open = signal(false);
  protected readonly toggle = (value: boolean) => !value;

  constructor() {
    this.hub.notifications$
      .pipe(takeUntilDestroyed())
      .subscribe((item) =>
        this.items.update((items) =>
          [item, ...items.filter((value) => value.id !== item.id)].slice(0, 30),
        ),
      );
    effect(() => {
      if (!this.auth.user()) {
        this.items.set([]);
        this.open.set(false);
      }
    });
  }
}
