import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly messages = signal<Toast[]>([]);
  private nextId = 0;

  readonly toasts = this.messages.asReadonly();

  success(message: string): void {
    this.add(message, 'success');
  }

  error(message: string): void {
    this.add(message, 'error');
  }

  info(message: string): void {
    this.add(message, 'info');
  }

  remove(id: number): void {
    this.messages.update((items) => items.filter((item) => item.id !== id));
  }

  private add(message: string, type: Toast['type']): void {
    const id = ++this.nextId;
    this.messages.update((items) => [...items.slice(-3), { id, message, type }]);
    setTimeout(() => this.remove(id), type === 'error' ? 8000 : 4500);
  }
}
