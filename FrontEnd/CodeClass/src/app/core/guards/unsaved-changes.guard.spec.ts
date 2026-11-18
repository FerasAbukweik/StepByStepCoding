import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { afterEach, describe, expect, it } from 'vitest';
import { unsavedChangesGuard } from './unsaved-changes.guard';
import { DialogService } from '../services/local/dialog-service';

function check(dirty: boolean) {
  return TestBed.runInInjectionContext(() =>
    unsavedChangesGuard(
      { hasUnsavedChanges: () => dirty },
      {} as ActivatedRouteSnapshot,
      {} as RouterStateSnapshot,
      {} as RouterStateSnapshot,
    ),
  );
}

describe('Unsaved changes guard', () => {
  afterEach(() => TestBed.resetTestingModule());

  it('allows navigation immediately when there is no draft', () => {
    expect(check(false)).toBe(true);
    expect(TestBed.inject(DialogService).confirmation()).toBeNull();
  });

  it('waits for the shared confirmation and preserves the page when cancelled', async () => {
    const decision = check(true);
    const dialogs = TestBed.inject(DialogService);
    expect(dialogs.confirmation()?.heading).toBe('Leave without saving?');
    dialogs.resolve(false);
    await expect(decision).resolves.toBe(false);
  });

  it('keeps the latest navigation waiting on the same confirmation', async () => {
    const first = check(true);
    const second = check(true);
    expect(second).toBe(first);
    TestBed.inject(DialogService).resolve(true);
    await expect(second).resolves.toBe(true);
  });

  it('allows navigation after the user confirms leaving', async () => {
    const decision = check(true);
    TestBed.inject(DialogService).resolve(true);
    await expect(decision).resolves.toBe(true);
  });
});
