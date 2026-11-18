import { catchErrorToast } from './catch-error-toast';
import { finalize, of, Subject, throwError } from 'rxjs';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ToastService } from '../services/local/toast-service';

describe('catchErrorToast', () => {
  let service: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();
    service = new ToastService();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('passes successful values through without adding a toast', () => {
    const next = vi.fn();

    of({ id: 'lecture' }).pipe(catchErrorToast('Could not load.', service)).subscribe(next);

    expect(next).toHaveBeenCalledWith({ id: 'lecture' });
    expect(service.toasts()).toEqual([]);
  });

  it('shows the supplied message once and completes without a success or error callback', () => {
    const next = vi.fn();
    const error = vi.fn();
    const complete = vi.fn();
    const finished = vi.fn();

    throwError(() => new Error('Internal details'))
      .pipe(catchErrorToast('Could not save the lecture.', service), finalize(finished))
      .subscribe({ next, error, complete });

    expect(service.toasts()).toEqual([
      { id: 1, type: 'error', message: 'Could not save the lecture.' },
    ]);
    expect(next).not.toHaveBeenCalled();
    expect(error).not.toHaveBeenCalled();
    expect(complete).toHaveBeenCalledOnce();
    expect(finished).toHaveBeenCalledOnce();

    vi.advanceTimersByTime(8000);
    expect(service.toasts()).toEqual([]);
  });

  it('adds no toast until the source fails and adds none on cancellation', () => {
    const source = new Subject<number>();
    const operation = source.pipe(catchErrorToast('Could not load.', service));

    expect(service.toasts()).toEqual([]);
    const subscription = operation.subscribe();
    subscription.unsubscribe();
    source.error(new Error('Late failure'));

    expect(service.toasts()).toEqual([]);
  });
});
