import { ToastService } from '../../core/services/local/toast-service';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { UsersService } from '../../core/services/data/users-service';
import { AuthService } from '../../core/services/data/auth-service';

@Component({
  selector: 'app-users',
  imports: [ReactiveFormsModule, RouterLink],
  providers: [UsersService],
  templateUrl: './users.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsersComponent {
  protected readonly service = inject(UsersService);
  protected readonly auth = inject(AuthService);
  protected readonly created = signal('');
  protected readonly roleError = signal(false);
  private readonly toast = inject(ToastService);
  private readonly destroy = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder).nonNullable;

  protected readonly form = this.formBuilder.group({
    fullName: ['', [Validators.required, Validators.maxLength(250)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128)]],
    instructor: false,
    student: true,
  });

  save(): void {
    this.form.markAllAsTouched();
    const value = this.form.getRawValue();
    const roles = [
      ...(this.auth.hasRole('Admin') && value.instructor ? [2] : []),
      ...(value.student ? [3] : []),
    ];
    this.roleError.set(!roles.length);
    if (this.form.invalid || !roles.length || this.service.saving() || !value.fullName.trim())
      return;
    this.service
      .create({
        fullName: value.fullName.trim(),
        email: value.email.trim(),
        password: value.password,
        roles,
      })
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe(() => {
        this.toast.success('Account created.');
        this.created.set(value.email.trim());
        this.form.reset({
          fullName: '',
          email: '',
          password: '',
          instructor: false,
          student: true,
        });
      });
  }
}
