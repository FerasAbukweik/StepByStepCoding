import { inject } from '@angular/core';
import { DialogService } from '../services/local/dialog-service';
import { CanDeactivateFn } from '@angular/router';

export interface EditableCodePage {
  hasUnsavedChanges(): boolean;
}

export const unsavedChangesGuard: CanDeactivateFn<EditableCodePage> = (component) =>
  !component.hasUnsavedChanges() ||
  inject(DialogService).confirm({
    heading: 'Leave without saving?',
    message: 'Your code has unsaved changes. Leave this page?',
    label: 'Leave page',
  });
