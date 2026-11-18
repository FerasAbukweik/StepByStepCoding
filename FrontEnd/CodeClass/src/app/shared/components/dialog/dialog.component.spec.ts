import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { DialogComponent } from './dialog.component';

@Component({
  imports: [DialogComponent],
  template: `<app-dialog
    heading="Edit course"
    [busy]="busy()"
    (closed)="closedCount = closedCount + 1"
  >
    <form>
      <label>Course name<input value="Programming" /></label>
    </form>
  </app-dialog>`,
})
class HostComponent {
  readonly busy = signal(false);
  closedCount = 0;
}

describe('Shared dialog', () => {
  const originalShow = Object.getOwnPropertyDescriptor(HTMLDialogElement.prototype, 'showModal');
  const originalClose = Object.getOwnPropertyDescriptor(HTMLDialogElement.prototype, 'close');
  let fixture: ComponentFixture<HostComponent>;
  let dialog: HTMLDialogElement;
  let show: ReturnType<typeof vi.fn>;
  let close: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    show = vi.fn(function (this: HTMLDialogElement) {
      this.open = true;
    });
    close = vi.fn(function (this: HTMLDialogElement) {
      this.open = false;
    });
    Object.defineProperty(HTMLDialogElement.prototype, 'showModal', {
      configurable: true,
      value: show,
    });
    Object.defineProperty(HTMLDialogElement.prototype, 'close', {
      configurable: true,
      value: close,
    });
    await TestBed.configureTestingModule({ imports: [HostComponent] }).compileComponents();
    fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    TestBed.tick();
    await fixture.whenStable();
    dialog = fixture.nativeElement.querySelector('dialog');
    vi.spyOn(dialog, 'getBoundingClientRect').mockReturnValue(new DOMRect(100, 100, 400, 300));
  });

  afterEach(() => {
    if (fixture && !fixture.componentRef.hostView.destroyed) fixture.destroy();
    TestBed.resetTestingModule();
    vi.restoreAllMocks();
    if (originalShow) Object.defineProperty(HTMLDialogElement.prototype, 'showModal', originalShow);
    else Reflect.deleteProperty(HTMLDialogElement.prototype, 'showModal');
    if (originalClose) Object.defineProperty(HTMLDialogElement.prototype, 'close', originalClose);
    else Reflect.deleteProperty(HTMLDialogElement.prototype, 'close');
  });

  it('opens a labelled native dialog and projects the supplied HTML', () => {
    expect(show).toHaveBeenCalledOnce();
    expect(dialog.open).toBe(true);
    expect(dialog.getAttribute('aria-label')).toBe('Edit course');
    expect(dialog.querySelector('input')?.value).toBe('Programming');
    fixture.detectChanges();
    expect(show).toHaveBeenCalledOnce();
  });

  it('requests dismissal from the close button or Escape', () => {
    dialog.querySelector<HTMLButtonElement>('button')!.click();
    expect(fixture.componentInstance.closedCount).toBe(1);
    const escape = new Event('cancel', { cancelable: true });
    dialog.dispatchEvent(escape);
    expect(escape.defaultPrevented).toBe(true);
    expect(fixture.componentInstance.closedCount).toBe(2);
  });

  it('dismisses only on the backdrop, preserving interactions within the dialog', () => {
    dialog.dispatchEvent(new MouseEvent('click', { bubbles: true, clientX: 200, clientY: 200 }));
    dialog.querySelector('input')!.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    expect(fixture.componentInstance.closedCount).toBe(0);
    dialog.dispatchEvent(new MouseEvent('click', { bubbles: true, clientX: 20, clientY: 20 }));
    expect(fixture.componentInstance.closedCount).toBe(1);
  });

  it('blocks all dismissal paths while an operation is running', () => {
    fixture.componentInstance.busy.set(true);
    fixture.detectChanges();
    const button = dialog.querySelector<HTMLButtonElement>('button')!;
    expect(button.disabled).toBe(true);
    button.click();
    dialog.dispatchEvent(new Event('cancel', { cancelable: true }));
    dialog.dispatchEvent(new MouseEvent('click', { clientX: 20, clientY: 20 }));
    expect(fixture.componentInstance.closedCount).toBe(0);
  });

  it('closes the native dialog when its owner is destroyed', () => {
    fixture.destroy();
    expect(close).toHaveBeenCalledOnce();
    expect(dialog.open).toBe(false);
  });
});
