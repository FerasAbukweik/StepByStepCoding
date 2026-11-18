import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  DestroyRef,
  inject,
  afterNextRender,
  input,
  output,
  viewChild,
} from '@angular/core';

@Component({
  selector: 'app-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './dialog.component.html',
})
export class DialogComponent {
  readonly heading = input.required<string>();
  readonly busy = input(false);
  readonly closed = output<void>();
  private readonly destroy = inject(DestroyRef);
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  constructor() {
    afterNextRender(() => {
      const dialog = this.dialog().nativeElement;
      dialog.showModal();
      this.destroy.onDestroy(() => {
        if (dialog.open) dialog.close();
      });
    });
  }

  dismiss(): void {
    if (!this.busy()) this.closed.emit();
  }

  cancel(event: Event): void {
    event.preventDefault();
    this.dismiss();
  }

  backdrop(event: MouseEvent): void {
    if (event.target !== this.dialog().nativeElement || this.busy()) return;
    const bounds = this.dialog().nativeElement.getBoundingClientRect();
    if (
      event.clientX < bounds.left ||
      event.clientX > bounds.right ||
      event.clientY < bounds.top ||
      event.clientY > bounds.bottom
    )
      this.dismiss();
  }
}
