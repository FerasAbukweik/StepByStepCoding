import { formatDate } from '@angular/common';
import { CreatedDTO } from '../../../../core/DTO/common/created.dto';
import { AuthService } from '../../../../core/services/data/auth-service';
import {
  computed,
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, finalize, tap } from 'rxjs';
import { DialogComponent } from '../../../../shared/components/dialog/dialog.component';
import { ExamsService } from '../../../../core/services/data/exams-service';
import { SectionsService } from '../../../../core/services/data/sections-service';
import { ExamDetailDTO } from '../../../../core/DTO/exams/exam-detail.dto';
import { SectionListDTO } from '../../../../core/DTO/sections/section-list.dto';
import { ToastService } from '../../../../core/services/local/toast-service';
import { catchErrorToast } from '../../../../core/Utilities/catch-error-toast';
import { apiError } from '../../../../core/utils/api-error';

@Component({
  selector: 'app-exam-form',
  imports: [ReactiveFormsModule, DialogComponent],
  templateUrl: './exam-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExamFormComponent implements OnInit {
  readonly exam = input<ExamDetailDTO | null>(null);
  readonly sectionId = input('');
  readonly closed = output<void>();
  readonly saved = output<string>();
  protected readonly sections = signal<SectionListDTO[]>([]);
  protected readonly saving = signal(false);
  protected readonly sectionsLoading = signal(false);
  protected readonly moreSections = signal(true);
  private sectionPage = 0;
  private sectionCount = 0;
  private readonly localError = signal('');
  protected readonly error = computed(() => this.localError() || this.examsData.actionError());
  private readonly auth = inject(AuthService);
  private readonly examsData = inject(ExamsService);
  private readonly sectionsData = inject(SectionsService);
  private readonly toast = inject(ToastService);
  private readonly destroy = inject(DestroyRef);

  private readonly formBuilder = inject(FormBuilder).nonNullable;

  protected readonly form = this.formBuilder.group({
    sectionId: ['', [Validators.required]],
    title: ['', [Validators.required, Validators.maxLength(250)]],
    instructions: ['', [Validators.maxLength(8000)]],
    startsAt: ['', [Validators.required]],
    deadline: ['', [Validators.required]],
    durationMinutes: [60, [Validators.required, Validators.min(1), Validators.max(1440)]],
  });

  ngOnInit(): void {
    const exam = this.exam();
    if (exam)
      this.form.patchValue({
        ...exam,
        instructions: exam.instructions ?? '',
        startsAt: formatDate(exam.startsAt, "yyyy-MM-dd'T'HH:mm", 'en-US'),
        deadline: formatDate(exam.deadline, "yyyy-MM-dd'T'HH:mm", 'en-US'),
      });
    else this.form.controls.sectionId.setValue(this.sectionId());
    if (!exam) {
      this.loadSections();
      if (this.sectionId()) {
        this.sectionsData
          .get(this.sectionId())
          .pipe(
            catchErrorToast('Could not load the selected section.', this.toast),
            takeUntilDestroyed(this.destroy),
          )
          .subscribe((section) =>
            this.sections.update((items) =>
              items.some((item) => item.id === section.id) ? items : [section, ...items],
            ),
          );
      }
    }
  }

  loadSections(): void {
    if (this.sectionsLoading()) return;
    this.sectionsLoading.set(true);
    const page = this.sectionPage + 1;
    this.sectionsData
      .list(page)
      .pipe(
        tap({ error: (error) => this.localError.set(apiError(error)) }),
        catchErrorToast('Could not load course sections.', this.toast),
        finalize(() => this.sectionsLoading.set(false)),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe((result) => {
        this.sectionPage = page;
        this.sectionCount += result.items.length;
        this.moreSections.set(this.sectionCount < result.totalCount);
        const sections = result.items.filter(
          (section) => this.auth.hasRole('Admin') || section.instructorId === this.auth.user()?.id,
        );
        this.sections.update((items) => [
          ...new Map([...items, ...sections].map((item) => [item.id, item])).values(),
        ]);
      });
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.saving()) return;
    const value = this.form.getRawValue();
    if (!value.title.trim()) {
      this.localError.set('Enter an exam title.');
      return;
    }
    if (new Date(value.startsAt).getTime() >= new Date(value.deadline).getTime()) {
      this.localError.set('The deadline must follow the opening time.');
      return;
    }
    this.saving.set(true);
    this.localError.set('');
    const request = {
      title: value.title.trim(),
      instructions: value.instructions.trim() || null,
      startsAt: new Date(value.startsAt).toISOString(),
      deadline: new Date(value.deadline).toISOString(),
      durationMinutes: value.durationMinutes,
    };
    const exam = this.exam();
    const action: Observable<unknown> = exam
      ? this.examsData.update(exam.id, { ...request, version: exam.version })
      : this.examsData.create({ ...request, sectionId: value.sectionId });

    action
      .pipe(
        tap({ error: (error) => this.localError.set(apiError(error)) }),
        catchErrorToast('Could not save the exam.', this.toast),
        finalize(() => this.saving.set(false)),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe((result) => {
        this.toast.success(exam ? 'Exam updated.' : 'Exam created.');
        this.saved.emit(exam?.id ?? (result as CreatedDTO).id);
      });
  }
}
