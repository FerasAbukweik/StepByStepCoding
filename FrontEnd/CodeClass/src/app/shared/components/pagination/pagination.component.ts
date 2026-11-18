import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `@if (total() > 0) {
    <nav
      aria-label="Pagination"
      class="mt-5 flex flex-wrap items-center justify-between gap-3 text-sm"
    >
      <p class="text-textSecondary">{{ first() }}–{{ last() }} of {{ total() }}</p>
      <div class="flex items-center gap-3">
        <button
          class="button-secondary"
          [disabled]="busy() || page() <= 1"
          (click)="changed.emit(page() - 1)"
        >
          Previous
        </button>
        <span class="text-textMuted">Page {{ page() }}</span>
        <button
          class="button-secondary"
          [disabled]="busy() || last() >= total()"
          (click)="changed.emit(page() + 1)"
        >
          Next
        </button>
      </div>
    </nav>
  }`,
})
export class PaginationComponent {
  readonly page = input(1);
  readonly total = input(0);
  readonly busy = input(false);
  readonly changed = output<number>();
  protected readonly first = computed(() => (this.page() - 1) * 20 + 1);
  protected readonly last = computed(() => Math.min(this.page() * 20, this.total()));
}
