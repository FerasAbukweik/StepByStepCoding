import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-state-panel',
  template: `@if (loading()) {
      <div class="py-5 space-y-4" role="status" aria-label="Loading">
        <div class="h-3 w-2/3 bg-borderSubtle rounded"></div>
        <div class="h-3 w-full bg-borderSubtle rounded"></div>
        <div class="h-3 w-4/5 bg-borderSubtle rounded"></div>
      </div>
    } @else {
      <div class="border-l-2 border-statusError bg-surface px-5 py-5 my-4" role="alert">
        <h2 class="text-sm font-semibold mb-2">Couldn't load this page.</h2>
        <p class="text-sm text-textSecondary mb-4">{{ message() }}</p>
        <button type="button" class="button-secondary" (click)="retry.emit()">Try again</button>
      </div>
    }`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatePanelComponent {
  readonly loading = input(false);
  readonly message = input('Please try again.');
  readonly retry = output<void>();
}
