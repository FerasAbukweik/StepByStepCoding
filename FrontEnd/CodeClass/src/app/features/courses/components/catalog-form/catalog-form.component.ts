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
import { Observable, finalize } from 'rxjs';
import { DialogComponent } from '../../../../shared/components/dialog/dialog.component';
import { UserSelectComponent } from '../../../../shared/components/user-select/user-select.component';
import { CourseListDTO } from '../../../../core/DTO/courses/course-list.dto';
import { SectionListDTO } from '../../../../core/DTO/sections/section-list.dto';
import { CoursesService } from '../../../../core/services/data/courses-service';
import { SectionsService } from '../../../../core/services/data/sections-service';
import { catchErrorToast } from '../../../../core/Utilities/catch-error-toast';
import { ToastService } from '../../../../core/services/local/toast-service';

@Component({
  selector: 'app-catalog-form',
  imports: [ReactiveFormsModule, DialogComponent, UserSelectComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<app-dialog
    [heading]="(item() ? 'Edit ' : 'New ') + type()"
    [busy]="saving()"
    (closed)="closed.emit()"
  >
    <form [formGroup]="form" (ngSubmit)="save()" class="space-y-5">
      @if (type() === 'section' && !item()) {
        <label class="form-label"
          >Course<select class="form-input mt-2" formControlName="courseId">
            <option value="">Select a course</option>
            @for (course of courses(); track course.id) {
              <option [value]="course.id">{{ course.name }}</option>
            }
          </select></label
        >
      }
      @if (type() === 'section' && !item() && (coursesLoading() || moreCourses())) {
        <button
          type="button"
          class="button-secondary"
          [disabled]="coursesLoading()"
          (click)="loadCourses()"
        >
          {{ coursesLoading() ? 'Loading courses…' : 'Load more courses' }}
        </button>
      }
      <label class="form-label"
        >{{ type() === 'course' ? 'Course name' : 'Section name'
        }}<input class="form-input mt-2" formControlName="name" maxlength="150"
      /></label>
      @if (type() === 'section') {
        <app-user-select
          role="Instructor"
          [selectedId]="form.controls.instructorId.value"
          [selectedName]="instructorName()"
          (selected)="form.controls.instructorId.setValue($event)"
        />
      }
      @if (form.invalid && form.touched) {
        <p class="text-sm text-statusError" role="alert">Complete the required fields.</p>
      }
      @if (error()) {
        <p class="text-sm text-statusError" role="alert">{{ error() }}</p>
      }
      <footer class="flex justify-end gap-3">
        <button
          type="button"
          class="button-secondary"
          [disabled]="saving()"
          (click)="closed.emit()"
        >
          Cancel</button
        ><button class="button-primary" [disabled]="saving()">
          {{ saving() ? 'Saving…' : 'Save ' + type() }}
        </button>
      </footer>
    </form>
  </app-dialog>`,
})
export class CatalogFormComponent implements OnInit {
  readonly type = input.required<'course' | 'section'>();
  readonly item = input<CourseListDTO | SectionListDTO | null>(null);
  readonly closed = output<void>();
  readonly saved = output<void>();
  private readonly coursesData = inject(CoursesService);
  private readonly sectionsData = inject(SectionsService);
  protected readonly saving = computed(
    () => this.coursesData.saving() || this.sectionsData.saving(),
  );
  protected readonly error = computed(
    () => this.coursesData.actionError() || this.sectionsData.actionError(),
  );
  protected readonly courses = signal<CourseListDTO[]>([]);
  protected readonly coursesLoading = signal(false);
  protected readonly moreCourses = signal(true);
  private coursePage = 0;
  private courseCount = 0;
  private readonly destroy = inject(DestroyRef);
  private readonly toast = inject(ToastService);
  private readonly formBuilder = inject(FormBuilder).nonNullable;

  protected readonly form = this.formBuilder.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    courseId: '',
    instructorId: '',
  });

  ngOnInit(): void {
    const item = this.item();
    if (item) this.form.patchValue(item);
    if (this.type() === 'section') {
      this.form.controls.instructorId.addValidators(Validators.required);
      this.form.controls.instructorId.updateValueAndValidity();
      if (!item) {
        this.form.controls.courseId.addValidators(Validators.required);
        this.form.controls.courseId.updateValueAndValidity();
      }
      if (!item) this.loadCourses();
    }
  }

  loadCourses(): void {
    if (this.coursesLoading()) return;
    this.coursesLoading.set(true);
    const page = this.coursePage + 1;
    this.coursesData
      .list(page)
      .pipe(
        catchErrorToast('Could not load courses.', this.toast),
        finalize(() => this.coursesLoading.set(false)),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe((result) => {
        this.coursePage = page;
        this.courseCount += result.items.length;
        this.moreCourses.set(this.courseCount < result.totalCount);
        this.courses.update((items) => [...items, ...result.items]);
      });
  }

  instructorName(): string {
    return (this.item() as SectionListDTO | null)?.instructorName ?? '';
  }

  save(): void {
    this.form.updateValueAndValidity();
    this.form.markAllAsTouched();
    if (this.form.invalid || this.saving()) return;
    const value = this.form.getRawValue();
    if (!value.name.trim()) return;
    const item = this.item();
    let action: Observable<unknown>;
    if (this.type() === 'course') {
      action = item
        ? this.coursesData.update(item.id, {
            name: value.name.trim(),
            version: item.version,
          })
        : this.coursesData.create({ name: value.name.trim() });
    } else {
      const section = { name: value.name.trim(), instructorId: value.instructorId };
      action = item
        ? this.sectionsData.update(item.id, { ...section, version: item.version })
        : this.sectionsData.create({ ...section, courseId: value.courseId });
    }
    action.pipe(takeUntilDestroyed(this.destroy)).subscribe(() => {
      this.toast.success('Saved successfully.');
      this.saved.emit();
    });
  }
}
