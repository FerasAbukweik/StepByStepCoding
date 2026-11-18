import { Subscription } from 'rxjs';
import {
  computed,
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CourseListComponent } from '../../shared/components/course-list/course-list.component';
import { StatePanelComponent } from '../../shared/components/state-panel/state-panel.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { CatalogFormComponent } from './components/catalog-form/catalog-form.component';
import { CoursesService } from '../../core/services/data/courses-service';
import { AuthService } from '../../core/services/data/auth-service';
import { SectionsService } from '../../core/services/data/sections-service';
import { ToastService } from '../../core/services/local/toast-service';
import { catchErrorToast } from '../../core/Utilities/catch-error-toast';
import { CourseListDTO } from '../../core/DTO/courses/course-list.dto';
import { SectionListDTO } from '../../core/DTO/sections/section-list.dto';

@Component({
  selector: 'app-courses',
  imports: [
    CourseListComponent,
    StatePanelComponent,
    PaginationComponent,
    ConfirmDialogComponent,
    CatalogFormComponent,
  ],
  providers: [CoursesService, SectionsService],
  templateUrl: './courses.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CoursesComponent implements OnInit {
  protected readonly coursesData = inject(CoursesService);
  protected readonly sectionsData = inject(SectionsService);
  protected readonly service = computed(() =>
    this.tab() === 'courses' ? this.coursesData : this.sectionsData,
  );
  protected readonly saving = computed(
    () => this.coursesData.saving() || this.sectionsData.saving(),
  );
  protected readonly actionError = computed(
    () => this.coursesData.actionError() || this.sectionsData.actionError(),
  );
  private readonly toast = inject(ToastService);
  protected readonly auth = inject(AuthService);
  protected readonly tab = signal<'sections' | 'courses'>('sections');
  protected readonly formType = signal<'course' | 'section' | null>(null);
  protected readonly editing = signal<CourseListDTO | SectionListDTO | null>(null);
  protected readonly deleting = signal<{
    type: 'course' | 'section';
    item: CourseListDTO | SectionListDTO;
  } | null>(null);
  private readonly destroy = inject(DestroyRef);
  private request?: Subscription;

  ngOnInit(): void {
    this.load();
  }

  load(page = 1): void {
    this.request?.unsubscribe();
    this.request = this.service()
      .load(page)
      .pipe(
        catchErrorToast('Could not load courses and sections.', this.toast),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe();
  }

  selectTab(tab: 'sections' | 'courses'): void {
    this.tab.set(tab);
    this.load();
  }

  form(type: 'course' | 'section', item: CourseListDTO | SectionListDTO | null = null): void {
    this.coursesData.actionError.set('');
    this.sectionsData.actionError.set('');
    this.editing.set(item);
    this.formType.set(type);
  }

  saved(): void {
    this.formType.set(null);
    this.load(this.service().page());
  }

  remove(): void {
    const target = this.deleting();
    if (!target || this.saving()) return;
    const api = target.type === 'course' ? this.coursesData : this.sectionsData;
    api
      .remove(target.item.id, target.item.version)
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe(() => {
        this.toast.success('Deleted successfully.');
        this.deleting.set(null);
        this.saved();
      });
  }
}
