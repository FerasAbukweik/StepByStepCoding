import { AuthService } from '../../core/services/data/auth-service';
import { DatePipe } from '@angular/common';
import { forkJoin } from 'rxjs';
import { catchErrorToast } from '../../core/Utilities/catch-error-toast';
import { ToastService } from '../../core/services/local/toast-service';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { CoursesService } from '../../core/services/data/courses-service';
import { SectionsService } from '../../core/services/data/sections-service';
import { LecturesService } from '../../core/services/data/lectures-service';
import { ExamsService } from '../../core/services/data/exams-service';
import { LectureTableComponent } from '../../shared/components/lecture-table/lecture-table.component';
import { ExamTableComponent } from '../../shared/components/exam-table/exam-table.component';
import { CourseListComponent } from '../../shared/components/course-list/course-list.component';
import { StatePanelComponent } from '../../shared/components/state-panel/state-panel.component';
import { VisibleDirective } from '../../shared/directives/visible.directive';

@Component({
  selector: 'app-home',
  imports: [
    DatePipe,
    RouterLink,
    LectureTableComponent,
    ExamTableComponent,
    CourseListComponent,
    StatePanelComponent,
    VisibleDirective,
  ],
  templateUrl: './home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {
  protected readonly auth = inject(AuthService);
  protected readonly coursesData = inject(CoursesService);
  protected readonly sectionsData = inject(SectionsService);
  protected readonly lecturesData = inject(LecturesService);
  protected readonly examsData = inject(ExamsService);
  private readonly destroy = inject(DestroyRef);
  private readonly toast = inject(ToastService);
  protected readonly loading = computed(
    () =>
      this.coursesData.loading() ||
      this.sectionsData.loading() ||
      this.lecturesData.loading() ||
      this.examsData.loading(),
  );
  protected readonly error = computed(
    () =>
      this.coursesData.error() ||
      this.sectionsData.error() ||
      this.lecturesData.error() ||
      this.examsData.error(),
  );

  protected readonly upcoming = computed(() =>
    this.examsData
      .loadedItems()
      .filter((exam) => exam.status === 2 && new Date(exam.deadline).getTime() > Date.now())
      .sort((a, b) => a.deadline.localeCompare(b.deadline))
      .slice(0, 3),
  );

  reload(): void {
    forkJoin([
      this.coursesData.load(),
      this.sectionsData.load(),
      this.lecturesData.load(),
      this.examsData.load(),
    ])
      .pipe(catchErrorToast('Could not load your academic data.', this.toast))
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe();
  }

  loadMore(): void {
    this.lecturesData.more().pipe(takeUntilDestroyed(this.destroy)).subscribe();
  }
}
