import { EMPTY, MonoTypeOperatorFunction, catchError } from 'rxjs';
import { ToastService } from '../services/local/toast-service';

export function catchErrorToast<T>(
  message: string,
  toast: ToastService,
): MonoTypeOperatorFunction<T> {
  return catchError(() => {
    toast.error(message);

    // Complete without a value so success callbacks cannot run after a failure.
    return EMPTY;
  });
}
