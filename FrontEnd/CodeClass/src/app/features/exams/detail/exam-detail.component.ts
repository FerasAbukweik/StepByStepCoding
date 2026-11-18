import { AuthService } from '../../../core/services/data/auth-service';
import { apiError } from '../../../core/utils/api-error';
import { AnswerDraft } from '../models/answer-draft';
import { ProblemStatementComponent } from '../../../shared/components/problem-statement/problem-statement.component';
import { DialogComponent } from '../../../shared/components/dialog/dialog.component';
import { DatePipe } from '@angular/common';
import {
  computed,
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  HostListener,
  inject,
  input,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import {
  Observable,
  Subject,
  defer,
  finalize,
  forkJoin,
  of,
  switchMap,
  takeUntil,
  tap,
} from 'rxjs';
import { ExamsService } from '../../../core/services/data/exams-service';
import { ExamQuestionsService } from '../../../core/services/data/exam-questions-service';
import { AttemptsService } from '../../../core/services/data/attempts-service';
import { SubmissionsService } from '../../../core/services/data/submissions-service';
import { SectionsService } from '../../../core/services/data/sections-service';
import { StatePanelComponent } from '../../../shared/components/state-panel/state-panel.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ExamFormComponent } from '../components/exam-form/exam-form.component';
import { QuestionFormComponent } from '../components/question-form/question-form.component';
import { ExamWorkAreaComponent } from '../components/exam-work-area/exam-work-area.component';
import { ExamQuestionListDTO } from '../../../core/DTO/examquestions/exam-question-list.dto';
import { ExamQuestionDetailDTO } from '../../../core/DTO/examquestions/exam-question-detail.dto';
import { ExamAttemptListDTO } from '../../../core/DTO/examattempts/exam-attempt-list.dto';
import { catchErrorToast } from '../../../core/Utilities/catch-error-toast';
import { ToastService } from '../../../core/services/local/toast-service';

@Component({
  selector: 'app-exam-detail',
  imports: [
    ProblemStatementComponent,
    DialogComponent,
    DatePipe,
    RouterLink,
    StatePanelComponent,
    ConfirmDialogComponent,
    PaginationComponent,
    ExamFormComponent,
    QuestionFormComponent,
    ExamWorkAreaComponent,
  ],
  providers: [ExamsService, ExamQuestionsService, AttemptsService, SubmissionsService],
  templateUrl: './exam-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExamDetailComponent {
  readonly examId = input.required<string>();
  protected readonly examsData = inject(ExamsService);
  protected readonly questionsData = inject(ExamQuestionsService);
  protected readonly attemptsData = inject(AttemptsService);
  protected readonly submissionsData = inject(SubmissionsService);
  protected readonly sectionsData = inject(SectionsService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroy = inject(DestroyRef);
  private readonly reviewCancelled = new Subject<void>();

  protected readonly exam = this.examsData.current;
  protected readonly questions = this.questionsData.loadedItems;
  protected readonly attempts = this.attemptsData.pageItems;
  protected readonly attempt = this.attemptsData.current;
  protected readonly attemptPage = this.attemptsData.page;
  protected readonly attemptTotal = this.attemptsData.total;
  protected readonly studentName = signal('');
  protected readonly loading = signal(true);
  protected readonly reviewing = signal(false);
  protected readonly error = signal('');
  protected readonly actionBusy = signal(false);
  private readonly localError = signal('');
  protected readonly actionError = computed(
    () =>
      this.localError() ||
      this.examsData.actionError() ||
      this.questionsData.actionError() ||
      this.attemptsData.actionError(),
  );
  protected readonly saving = computed(
    () =>
      this.actionBusy() ||
      this.examsData.saving() ||
      this.questionsData.saving() ||
      this.attemptsData.saving() ||
      this.submissionsData.saving(),
  );
  protected readonly manager = computed(
    () =>
      this.auth.hasRole('Admin') ||
      (this.auth.hasRole('Instructor') && this.exam()?.instructorId === this.auth.user()?.id),
  );
  protected readonly now = signal(Date.now());
  protected readonly canStart = computed(
    () =>
      !this.manager() &&
      !this.attempt() &&
      !this.attemptsData.refreshRequired() &&
      this.exam()?.status === 2 &&
      this.now() >= new Date(this.exam()!.startsAt).getTime() &&
      this.now() < new Date(this.exam()!.deadline).getTime(),
  );

  protected readonly tab = signal<'questions' | 'attempts'>('questions');
  protected readonly editing = signal(false);
  protected readonly questionForm = signal(false);
  protected readonly editQuestion = signal<ExamQuestionDetailDTO | null>(null);
  protected readonly confirmation = signal<
    'start' | 'publish' | 'close' | 'delete' | 'delete-exam' | null
  >(null);
  protected readonly deleting = signal<ExamQuestionListDTO | null>(null);
  protected readonly drafts = signal<ReadonlyMap<string, AnswerDraft>>(new Map());
  protected readonly selectedQuestion = signal('');

  constructor() {
    const timer = setInterval(() => this.now.set(Date.now()), 1000);
    this.destroy.onDestroy(() => {
      clearInterval(timer);
      this.reviewCancelled.next();
      this.reviewCancelled.complete();
    });
    toObservable(this.examId)
      .pipe(
        switchMap((id) => this.loadExam(id)),
        takeUntilDestroyed(),
      )
      .subscribe();
  }

  reload(): void {
    this.loadExam(this.examId()).pipe(takeUntilDestroyed(this.destroy)).subscribe();
  }

  saved(): void {
    this.editing.set(false);
    this.questionForm.set(false);
    this.editQuestion.set(null);
  }

  newQuestion(): void {
    this.clearActionError();
    this.editQuestion.set(null);
    this.questionForm.set(true);
  }

  edit(question: ExamQuestionListDTO): void {
    this.questionsData
      .get(question.id)
      .pipe(
        catchErrorToast('Could not load this question.', this.toast),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe((detail) => {
        this.clearActionError();
        this.editQuestion.set(detail);
        this.questionForm.set(true);
      });
  }

  remove(question: ExamQuestionListDTO): void {
    this.clearActionError();
    this.deleting.set(question);
    this.confirmation.set('delete');
  }

  review(attempt: ExamAttemptListDTO): void {
    this.readAttempt(attempt).pipe(takeUntilDestroyed(this.destroy)).subscribe();
  }

  moreQuestions(): void {
    this.questionsData
      .more(this.examId())
      .pipe(
        catchErrorToast('Could not load more questions.', this.toast),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe();
  }

  moreAttempts(page: number): void {
    this.attemptsData
      .load(this.examId(), page)
      .pipe(
        catchErrorToast('Could not load exam attempts.', this.toast),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe();
  }

  confirmAction(): void {
    if (this.saving()) return;
    const exam = this.exam()!;
    const requested = this.confirmation();
    let action: Observable<unknown>;
    let message: string;
    switch (requested) {
      case 'start':
        action = this.attemptsData.start(exam.id).pipe(
          switchMap(() => this.questionsData.load(exam.id)),
          tap(() => this.submissionsData.reset()),
        );
        message = 'Attempt started.';
        break;
      case 'publish':
        action = this.examsData.publish(exam.id, exam.version);
        message = 'Exam published.';
        break;
      case 'close':
        action = this.examsData.close(exam.id, exam.version);
        message = 'Exam closed.';
        break;
      case 'delete-exam':
        action = this.examsData.remove(exam.id, exam.version);
        message = 'Draft deleted.';
        break;
      case 'delete': {
        const question = this.deleting()!;
        action = this.questionsData.remove(question.id, question.version);
        message = 'Question deleted.';
        break;
      }
      default:
        return;
    }
    this.actionBusy.set(true);
    this.clearActionError();
    action
      .pipe(
        tap({ error: (error) => this.localError.set(apiError(error)) }),
        catchErrorToast(
          'Could not complete this action. Refresh the exam and try again.',
          this.toast,
        ),
        finalize(() => this.actionBusy.set(false)),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe(() => {
        this.toast.success(message);
        this.confirmation.set(null);
        if (requested === 'delete-exam') void this.router.navigateByUrl('/exams');
      });
  }

  confirmationHeading(): string {
    return {
      start: 'Start timed attempt?',
      publish: 'Publish exam?',
      close: 'Close exam?',
      delete: 'Delete question?',
      'delete-exam': 'Delete draft exam?',
    }[this.confirmation() ?? 'delete'];
  }

  confirmationText(): string {
    switch (this.confirmation()) {
      case 'start':
        return 'The timer begins immediately and continues if you leave. You have one attempt and one submission per question. The exam deadline may shorten your available time.';
      case 'publish':
        return 'Enrolled students will be able to access this exam during its availability window. Published questions and settings cannot be edited.';
      case 'delete-exam':
        return 'This removes the empty draft exam. This cannot be undone.';
      case 'close':
        return 'Students will no longer be able to start attempts or submit answers. Only answers already submitted are saved.';
      default:
        return 'This question will be removed from the draft exam. This cannot be undone.';
    }
  }

  dateValue(value: string): number {
    return new Date(value).getTime();
  }

  hasUnsavedChanges(): boolean {
    return !this.manager() && [...this.drafts().values()].some((draft) => !!draft.code.trim());
  }

  @HostListener('window:beforeunload', ['$event'])
  beforeUnload(event: BeforeUnloadEvent): void {
    if (this.hasUnsavedChanges()) {
      event.preventDefault();
      event.returnValue = '';
    }
  }

  private loadExam(id: string) {
    return defer(() => {
      this.reviewCancelled.next();
      if (this.exam()?.id !== id) {
        this.drafts.set(new Map());
        this.selectedQuestion.set('');
        for (const data of [
          this.examsData,
          this.questionsData,
          this.attemptsData,
          this.submissionsData,
        ])
          data.reset();
      }
      this.loading.set(true);
      this.error.set('');
      return forkJoin({
        exam: this.examsData.refresh(id),
        questions: this.questionsData.load(id),
        attempts: this.attemptsData.load(id),
      });
    }).pipe(
      switchMap((data) => {
        const selected = this.manager()
          ? this.attempt()
          : data.attempts.items.find((item) => item.studentId === this.auth.user()?.id);
        return selected
          ? this.readAttempt({
              ...selected,
              studentName:
                'studentName' in selected ? String(selected.studentName) : this.studentName(),
            })
          : of(undefined);
      }),
      tap({ error: (error) => this.error.set(apiError(error)) }),
      catchErrorToast('Could not load this exam.', this.toast),
      finalize(() => this.loading.set(false)),
    );
  }

  private readAttempt(attempt: ExamAttemptListDTO) {
    this.reviewCancelled.next();
    this.reviewing.set(true);
    this.submissionsData.reset();
    return this.attemptsData.refresh(attempt.id).pipe(
      takeUntil(this.reviewCancelled),
      tap(() => this.studentName.set(attempt.studentName || 'Student')),
      tap({ error: (error) => this.error.set(apiError(error)) }),
      catchErrorToast('Could not load this attempt.', this.toast),
      finalize(() => this.reviewing.set(false)),
    );
  }

  private clearActionError(): void {
    this.localError.set('');
    this.examsData.actionError.set('');
    this.questionsData.actionError.set('');
    this.attemptsData.actionError.set('');
  }
}
