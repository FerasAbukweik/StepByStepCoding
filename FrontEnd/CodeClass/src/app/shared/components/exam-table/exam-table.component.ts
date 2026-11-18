import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { ExamListDTO } from '../../../core/DTO/exams/exam-list.dto';
import { SectionsService } from '../../../core/services/data/sections-service';

@Component({
  selector: 'app-exam-table',
  imports: [DatePipe, RouterLink],
  templateUrl: './exam-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExamTableComponent {
  readonly exams = input.required<ExamListDTO[]>();
  protected readonly sectionsData = inject(SectionsService);
  private readonly now = signal(Date.now());
  protected readonly openCount = computed(
    () => this.exams().filter((exam) => this.isOpen(exam)).length,
  );

  constructor() {
    const timer = setInterval(() => this.now.set(Date.now()), 30000);
    inject(DestroyRef).onDestroy(() => clearInterval(timer));
  }

  isOpen(exam: ExamListDTO): boolean {
    return (
      exam.status === 2 &&
      new Date(exam.startsAt).getTime() <= this.now() &&
      new Date(exam.deadline).getTime() > this.now()
    );
  }

  status(exam: ExamListDTO): string {
    return exam.status === 1
      ? 'Draft'
      : this.isOpen(exam)
        ? 'Open now'
        : new Date(exam.startsAt).getTime() > this.now()
          ? 'Upcoming'
          : 'Closed';
  }
}
