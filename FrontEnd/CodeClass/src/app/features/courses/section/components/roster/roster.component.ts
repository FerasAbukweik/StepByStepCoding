import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { EnrollmentsService } from '../../../../../core/services/data/enrollments-service';
import { catchErrorToast } from '../../../../../core/Utilities/catch-error-toast';
import { ToastService } from '../../../../../core/services/local/toast-service';
import { EnrollmentListDTO } from '../../../../../core/DTO/enrollments/enrollment-list.dto';
import { StatePanelComponent } from '../../../../../shared/components/state-panel/state-panel.component';
import { PaginationComponent } from '../../../../../shared/components/pagination/pagination.component';
import { DialogComponent } from '../../../../../shared/components/dialog/dialog.component';
import { ConfirmDialogComponent } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { UserSelectComponent } from '../../../../../shared/components/user-select/user-select.component';

@Component({
  selector: 'app-roster',
  imports: [
    RouterLink,
    StatePanelComponent,
    PaginationComponent,
    DialogComponent,
    ConfirmDialogComponent,
    UserSelectComponent,
  ],
  providers: [EnrollmentsService],
  templateUrl: './roster.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RosterComponent implements OnInit {
  readonly sectionId = input.required<string>();
  protected readonly service = inject(EnrollmentsService);
  protected readonly adding = signal(false);
  protected readonly studentId = signal('');
  protected readonly removing = signal<EnrollmentListDTO | null>(null);
  private readonly toast = inject(ToastService);
  private readonly destroy = inject(DestroyRef);

  ngOnInit(): void {
    this.load();
  }

  load(page = 1): void {
    this.service
      .load(this.sectionId(), page)
      .pipe(
        catchErrorToast('Could not load enrolled students.', this.toast),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe();
  }

  add(): void {
    if (!this.studentId() || this.service.saving()) return;
    this.service
      .create(this.sectionId(), this.studentId())
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe(() => {
        this.toast.success('Student enrolled.');
        this.adding.set(false);
        this.studentId.set('');
        this.load(1);
      });
  }

  toggle(item: EnrollmentListDTO): void {
    this.service
      .update(item.id, item.version, item.status !== 1)
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe(() => {
        this.toast.success('Enrollment updated.');
        this.load(this.service.page());
      });
  }

  remove(): void {
    const item = this.removing();
    if (!item || this.service.saving()) return;
    this.service
      .remove(item.id, item.version)
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe(() => {
        this.toast.success('Enrollment removed.');
        this.removing.set(null);
        this.load(1);
      });
  }
}
