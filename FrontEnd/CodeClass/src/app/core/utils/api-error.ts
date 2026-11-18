import { HttpErrorResponse } from '@angular/common/http';

export function apiError(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) return 'Something went wrong. Please try again.';
  if (error.status === 0) return "Can't reach CodeClass. Check your connection and try again.";

  const body = error.error as {
    message?: string;
    title?: string;
    errors?: string[] | Record<string, string[]>;
  } | null;
  if (body?.errors) return Object.values(body.errors).flat().join(' ');
  return body?.message || body?.title || 'The request could not be completed. Please try again.';
}
