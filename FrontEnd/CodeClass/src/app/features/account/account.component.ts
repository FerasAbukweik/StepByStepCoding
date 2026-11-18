import { ChangeDetectionStrategy, Component, DestroyRef, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/data/auth-service';

@Component({
  selector: 'app-account',
  imports: [ReactiveFormsModule],
  templateUrl: './account.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountComponent {
  protected readonly auth = inject(AuthService);
  private readonly destroy = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder).nonNullable;

  protected readonly form = this.formBuilder.group(
    {
      currentPassword: ['', [Validators.required, Validators.maxLength(128)]],
      newPassword: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128)]],
      confirmation: ['', [Validators.required]],
    },
    {
      validators: (control) =>
        control.get('newPassword')?.value === control.get('confirmation')?.value
          ? null
          : { mismatch: true },
    },
  );

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.auth.passwordSaving()) return;
    const { currentPassword, newPassword } = this.form.getRawValue();
    this.auth
      .changePassword({ currentPassword, newPassword })
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe();
  }
}
