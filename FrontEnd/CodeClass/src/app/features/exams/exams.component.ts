import { catchErrorToast } from '../../core/Utilities/catch-error-toast';
import { ToastService } from '../../core/services/local/toast-service';
import { Subscription } from 'rxjs';
import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { ExamsService } from '../../core/services/data/exams-service';
import { AuthService } from '../../core/services/data/auth-service';
import { SectionsService } from '../../core/services/data/sections-service';
import { ExamListDTO } from '../../core/DTO/exams/exam-list.dto';
import { StatePanelComponent } from '../../shared/components/state-panel/state-panel.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { ExamFormComponent } from './components/exam-form/exam-form.component';

@Component({
  selector: 'app-exams',
  imports: [
    DatePipe,
    FormsModule,
    RouterLink,
    StatePanelComponent,
    PaginationComponent,
    ExamFormComponent,
  ],
  providers: [ExamsService],
  templateUrl: './exams.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExamsComponent implements OnInit {
  protected readonly service = inject(ExamsService);
  protected readonly auth = inject(AuthService);
  protected readonly sectionsData = inject(SectionsService);
  protected readonly creating = signal(false);
  readonly section = input('');
  protected sectionId = '';
  private readonly destroy = inject(DestroyRef);
  private request?: Subscription;
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.sectionId = this.section();
    this.load();
  }

  load(page = 1): void {
    this.request?.unsubscribe();
    this.request = this.service
      .load(page, { sectionId: this.sectionId || undefined })
      .pipe(catchErrorToast('Could not load exams.', this.toast), takeUntilDestroyed(this.destroy))
      .subscribe();
  }

  created(id: string): void {
    this.creating.set(false);
    void this.router.navigate(['/exams', id]);
  }

  status(exam: ExamListDTO): string {
    if (exam.status === 1) return 'Draft';
    if (exam.status === 3 || Date.now() >= new Date(exam.deadline).getTime()) return 'Closed';
    return Date.now() < new Date(exam.startsAt).getTime() ? 'Upcoming' : 'Open';
  }
}
