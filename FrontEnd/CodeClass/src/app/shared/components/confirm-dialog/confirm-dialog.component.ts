import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { DialogComponent } from '../dialog/dialog.component';

@Component({
  selector: 'app-confirm-dialog',
  imports: [DialogComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<app-dialog [heading]="heading()" [busy]="busy()" (closed)="closed.emit()">
    <p class="leading-relaxed text-textSecondary">{{ message() }}</p>
    @if (error()) {
      <p class="mt-4 text-sm text-statusError" role="alert">{{ error() }}</p>
    }
    <div class="mt-6 flex justify-end gap-3">
      <button class="button-secondary" [disabled]="busy()" (click)="closed.emit()">Cancel</button>
      <button class="button-primary" [disabled]="busy()" (click)="confirmed.emit()">
        {{ busy() ? 'Please wait…' : label() }}
      </button>
    </div>
  </app-dialog>`,
})
export class ConfirmDialogComponent {
  readonly heading = input.required<string>();
  readonly message = input.required<string>();
  readonly label = input('Confirm');
  readonly error = input('');
  readonly busy = input(false);
  readonly closed = output<void>();
  readonly confirmed = output<void>();
}
