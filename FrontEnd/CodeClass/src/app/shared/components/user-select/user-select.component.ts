import { apiError } from '../../../core/utils/api-error';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, tap } from 'rxjs';
import { UsersService } from '../../../core/services/data/users-service';
import { catchErrorToast } from '../../../core/Utilities/catch-error-toast';
import { ToastService } from '../../../core/services/local/toast-service';

@Component({
  selector: 'app-user-select',
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="space-y-2">
    <label class="form-label"
      >{{ role() === 'Instructor' ? 'Instructor' : 'Student' }}
      <div class="mt-2 flex gap-2">
        <input
          class="form-input"
          [(ngModel)]="term"
          [ngModelOptions]="{ standalone: true }"
          placeholder="Search by name or email"
          (keydown.enter)="$event.preventDefault(); search()"
        />
        <button type="button" class="button-secondary" [disabled]="loading()" (click)="search()">
          Search
        </button>
      </div>
    </label>
    <select
      class="form-input"
      aria-label="Select account"
      [value]="selectedId()"
      (change)="choose($event)"
      [disabled]="loading()"
    >
      <option value="">{{ loading() ? 'Loading accounts…' : 'Select an account' }}</option>
      @if (selectedId() && !hasSelection()) {
        <option [value]="selectedId()">{{ selectedName() || 'Current account' }}</option>
      }
      @for (user of users(); track user.id) {
        <option [value]="user.id">{{ user.fullName }} · {{ user.email }}</option>
      }
    </select>
    @if (!loading() && users().length === 20) {
      <p class="text-sm text-textMuted">
        Showing up to 20 matches. Search by name or email to narrow the results.
      </p>
    }
    @if (error()) {
      <p class="text-sm text-statusError" role="alert">{{ error() }}</p>
    }
    @if (!loading() && !error() && !users().length) {
      <p class="text-sm text-textMuted">
        No matching accounts. Create the account first if needed.
      </p>
    }
  </div>`,
})
export class UserSelectComponent implements OnInit {
  readonly role = input.required<'Instructor' | 'Student'>();
  readonly selectedId = input('');
  readonly selectedName = input('');
  readonly selected = output<string>();
  private readonly usersData = inject(UsersService);
  protected readonly users = this.usersData.loadedItems;
  protected readonly error = signal('');
  protected readonly loading = signal(false);
  protected term = '';
  private readonly toast = inject(ToastService);
  private readonly destroy = inject(DestroyRef);

  ngOnInit(): void {
    this.search();
  }

  hasSelection(): boolean {
    return this.users().some((user) => user.id === this.selectedId());
  }

  search(): void {
    if (this.loading()) return;
    this.loading.set(true);
    this.error.set('');
    this.usersData
      .search(this.role(), this.term.trim())
      .pipe(
        tap({ error: (error) => this.error.set(apiError(error)) }),
        catchErrorToast('Could not find matching accounts.', this.toast),
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe();
  }

  choose(event: Event): void {
    this.selected.emit((event.target as HTMLSelectElement).value);
  }
}
