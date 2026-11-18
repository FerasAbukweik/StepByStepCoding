import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { afterEach, describe, expect, it } from 'vitest';
import { DialogService } from './dialog-service';
import { App } from '../../../app';

describe('Dialog confirmations', () => {
  afterEach(() => TestBed.resetTestingModule());

  it('renders the global confirmation through the shared dialog and resolves approval', async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const dialogs = TestBed.inject(DialogService);
    const result = dialogs.confirm({
      heading: 'Replace workspace?',
      message: 'Your saved code will be replaced.',
      label: 'Replace code',
    });
    fixture.detectChanges();
    await fixture.whenStable();
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    expect(dialog.getAttribute('aria-label')).toBe('Replace workspace?');
    const buttons = [...dialog.querySelectorAll('button')];
    buttons.find((button) => button.textContent?.trim() === 'Replace code')!.click();
    await expect(result).resolves.toBe(true);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('dialog')).toBeNull();
    fixture.destroy();
  });

  it('resolves cancellation and allows another confirmation', async () => {
    const dialogs = TestBed.inject(DialogService);
    const first = dialogs.confirm({ heading: 'Leave?', message: 'A draft is unsaved.' });
    dialogs.resolve(false);
    await expect(first).resolves.toBe(false);
    expect(dialogs.confirmation()).toBeNull();
    const second = dialogs.confirm({ heading: 'Reload?', message: 'Reload the saved file.' });
    dialogs.resolve(true);
    await expect(second).resolves.toBe(true);
  });

  it('shares the decision for duplicate requests without opening another dialog', async () => {
    const dialogs = TestBed.inject(DialogService);
    const options = { heading: 'Leave?', message: 'Unsaved draft.', label: 'Leave page' };
    const first = dialogs.confirm(options);
    const second = dialogs.confirm({ ...options });
    expect(second).toBe(first);
    dialogs.resolve(true);
    await expect(first).resolves.toBe(true);
    await expect(second).resolves.toBe(true);
  });

  it('preserves the active confirmation when another action asks for one', async () => {
    const dialogs = TestBed.inject(DialogService);
    const active = dialogs.confirm({ heading: 'Keep this question', message: 'First action.' });
    await expect(
      dialogs.confirm({ heading: 'Second question', message: 'Second action.' }),
    ).resolves.toBe(false);
    expect(dialogs.confirmation()?.heading).toBe('Keep this question');
    dialogs.resolve(true);
    await expect(active).resolves.toBe(true);
  });
});
