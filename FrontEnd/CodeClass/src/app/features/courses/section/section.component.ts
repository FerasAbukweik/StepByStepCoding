import { catchErrorToast } from '../../../core/Utilities/catch-error-toast';
import { apiError } from '../../../core/utils/api-error';
import { computed } from '@angular/core';
import { RosterComponent } from './components/roster/roster.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  input,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { finalize, forkJoin, switchMap, tap } from 'rxjs';
import { SectionsService } from '../../../core/services/data/sections-service';
import { LecturesService } from '../../../core/services/data/lectures-service';
import { ExamsService } from '../../../core/services/data/exams-service';
import { AuthService } from '../../../core/services/data/auth-service';
import { ToastService } from '../../../core/services/local/toast-service';
import { LectureListDTO } from '../../../core/DTO/lectures/lecture-list.dto';
import { LectureTableComponent } from '../../../shared/components/lecture-table/lecture-table.component';
import { ExamTableComponent } from '../../../shared/components/exam-table/exam-table.component';
import { StatePanelComponent } from '../../../shared/components/state-panel/state-panel.component';
import { VisibleDirective } from '../../../shared/directives/visible.directive';
import { LectureFormComponent } from './components/lecture-form/lecture-form.component';

@Component({
  selector: 'app-section',
  imports: [
    RouterLink,
    LectureTableComponent,
    ExamTableComponent,
    StatePanelComponent,
    VisibleDirective,
    LectureFormComponent,
    RosterComponent,
    ConfirmDialogComponent,
  ],
  providers: [SectionsService, LecturesService, ExamsService],
  templateUrl: './section.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SectionComponent {
  readonly sectionId = input.required<string>();
  protected readonly sectionsData = inject(SectionsService);
  protected readonly lecturesData = inject(LecturesService);
  protected readonly examsData = inject(ExamsService);
  protected readonly section = this.sectionsData.current;
  protected readonly lectures = this.lecturesData.loadedItems;
  protected readonly exams = this.examsData.loadedItems;
  protected readonly saving = this.lecturesData.saving;
  protected readonly loadingMore = this.lecturesData.loading;
  protected readonly hasMore = this.lecturesData.hasMore;
  protected readonly live = computed(() => this.lecturesData.live()[0]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly actionError = this.lecturesData.actionError;
  protected readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly destroy = inject(DestroyRef);
  protected readonly tab = signal<'lectures' | 'exams' | 'students'>('lectures');
  protected readonly canManage = computed(
    () =>
      this.auth.hasRole('Admin') ||
      (this.auth.hasRole('Instructor') && this.section()?.instructorId === this.auth.user()?.id),
  );
  protected readonly editingLecture = signal<LectureListDTO | null>(null);
  protected readonly pendingLecture = signal<LectureListDTO | null>(null);
  protected readonly deletingLecture = signal<LectureListDTO | null>(null);
  protected readonly showCreate = signal(false);

  constructor() {
    toObservable(this.sectionId)
      .pipe(
        switchMap((id) => this.loadSection(id)),
        takeUntilDestroyed(),
      )
      .subscribe();
  }

  load(): void {
    this.loadSection(this.sectionId()).pipe(takeUntilDestroyed(this.destroy)).subscribe();
  }

  moreExams(): void {
    if (this.examsData.loading() || !this.examsData.hasMore()) return;
    this.examsData
      .more({ sectionId: this.sectionId() })
      .pipe(
        catchErrorToast('Could not load more exams.', this.toast),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe();
  }

  more(): void {
    if (this.loadingMore() || !this.hasMore()) return;
    this.lecturesData
      .more({ sectionId: this.sectionId() })
      .pipe(
        catchErrorToast('Could not load more lectures.', this.toast),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe();
  }

  refresh(): void {
    this.showCreate.set(false);
    this.editingLecture.set(null);
  }

  edit(lecture: LectureListDTO): void {
    this.editingLecture.set(lecture);
    this.showCreate.set(true);
  }

  changeStatus(lecture: LectureListDTO): void {
    this.pendingLecture.set(lecture);
  }

  confirmStatus(): void {
    const lecture = this.pendingLecture();
    if (!lecture || this.saving()) return;
    const action =
      lecture.status === 1
        ? this.lecturesData.start(lecture.id, lecture.version)
        : this.lecturesData.end(lecture.id, lecture.version);
    action.pipe(takeUntilDestroyed(this.destroy)).subscribe(() => {
      this.pendingLecture.set(null);
      this.toast.success(lecture.status === 1 ? 'Lecture started.' : 'Lecture ended.');
      this.refresh();
    });
  }

  removeLecture(): void {
    const lecture = this.deletingLecture();
    if (!lecture || this.saving()) return;
    this.lecturesData
      .remove(lecture.id, lecture.version)
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe(() => {
        this.deletingLecture.set(null);
        this.toast.success('Lecture deleted.');
        this.refresh();
      });
  }
  private loadSection(id: string) {
    this.loading.set(true);
    this.error.set('');
    return forkJoin({
      section: this.sectionsData.get(id),
      lectures: this.lecturesData.load(1, { sectionId: id }),
      exams: this.examsData.load(1, { sectionId: id }),
    }).pipe(
      tap(({ section }) => this.sectionsData.current.set(section)),
      tap({ error: (error) => this.error.set(apiError(error)) }),
      catchErrorToast('Could not load this course section.', this.toast),
      finalize(() => this.loading.set(false)),
    );
  }
}
