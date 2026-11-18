import { Injectable, signal } from '@angular/core';

export interface ConfirmationOptions {
  heading: string;
  message: string;
  label?: string;
}

@Injectable({ providedIn: 'root' })
export class DialogService {
  private readonly pending = signal<ConfirmationOptions | null>(null);
  private decision?: Promise<boolean>;
  private complete?: (confirmed: boolean) => void;

  readonly confirmation = this.pending.asReadonly();

  confirm(options: ConfirmationOptions): Promise<boolean> {
    const current = this.pending();
    if (current) {
      const sameQuestion =
        current.heading === options.heading &&
        current.message === options.message &&
        current.label === options.label;
      return sameQuestion ? this.decision! : Promise.resolve(false);
    }

    this.decision = new Promise((resolve) => {
      this.complete = resolve;
    });
    this.pending.set(options);
    return this.decision;
  }

  resolve(confirmed: boolean): void {
    const complete = this.complete;
    this.complete = undefined;
    this.decision = undefined;
    this.pending.set(null);
    complete?.(confirmed);
  }
}
