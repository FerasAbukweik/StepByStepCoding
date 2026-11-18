import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  effect,
  inject,
  input,
  model,
  signal,
} from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, finalize, forkJoin, takeUntil, tap } from 'rxjs';
import { ExamsService } from '../../../../core/services/data/exams-service';
import { ExamQuestionsService } from '../../../../core/services/data/exam-questions-service';
import { AttemptsService } from '../../../../core/services/data/attempts-service';
import { SubmissionsService } from '../../../../core/services/data/submissions-service';
import { AuthService } from '../../../../core/services/data/auth-service';
import { ToastService } from '../../../../core/services/local/toast-service';
import { ProgrammingLanguage } from '../../../../core/enums/programming-language';
import { catchErrorToast } from '../../../../core/Utilities/catch-error-toast';
import { apiError } from '../../../../core/utils/api-error';
import { AnswerDraft } from '../../models/answer-draft';
import { CodeEditorComponent } from '../../../../shared/components/code-editor/code-editor.component';
import { ProblemStatementComponent } from '../../../../shared/components/problem-statement/problem-statement.component';
import { StatePanelComponent } from '../../../../shared/components/state-panel/state-panel.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-exam-work-area',
  imports: [
    DatePipe,
    FormsModule,
    ReactiveFormsModule,
    CodeEditorComponent,
    ProblemStatementComponent,
    StatePanelComponent,
    ConfirmDialogComponent,
  ],
  templateUrl: './exam-work-area.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExamWorkAreaComponent implements OnInit {
  readonly manager = input(false);
  readonly studentName = input('');
  readonly drafts = model<ReadonlyMap<string, AnswerDraft>>(new Map());
  readonly selectedQuestion = model('');

  private readonly examsData = inject(ExamsService);
  protected readonly questionsData = inject(ExamQuestionsService);
  private readonly attemptsData = inject(AttemptsService);
  private readonly submissionsData = inject(SubmissionsService);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly destroy = inject(DestroyRef);
  private readonly selectionCancelled = new Subject<void>();

  protected readonly attempt = this.attemptsData.current;
  protected readonly questions = this.questionsData.loadedItems;
  protected readonly question = this.questionsData.current;
  protected readonly submissions = this.submissionsData.loadedItems;
  protected readonly submission = this.submissionsData.current;
  protected readonly code = signal('');
  protected readonly language = signal<ProgrammingLanguage>(ProgrammingLanguage.Cpp);
  protected readonly questionLoading = signal(false);
  protected readonly questionError = signal('');
  protected readonly confirm = signal<'submit' | 'finish' | null>(null);
  protected readonly now = signal(Date.now());
  protected readonly saving = computed(
    () => this.submissionsData.saving() || this.attemptsData.saving(),
  );
  protected readonly syncRequired = computed(
    () => this.submissionsData.refreshRequired() || this.attemptsData.refreshRequired(),
  );
  protected readonly actionError = computed(() =>
    this.syncRequired()
      ? 'The action may have been saved. Refresh the exam before continuing.'
      : this.submissionsData.actionError() || this.attemptsData.actionError(),
  );
  protected readonly graded = computed(
    () => this.submissions().filter((item) => item.score !== null).length,
  );
  protected readonly score = computed(() =>
    this.submissions().reduce((sum, item) => sum + (item.score ?? 0), 0),
  );
  protected readonly remaining = computed(() =>
    Math.max(
      0,
      Math.ceil((new Date(this.attempt()?.expiresAt ?? 0).getTime() - this.now()) / 1000),
    ),
  );
  protected readonly active = computed(
    () =>
      !this.manager() &&
      this.attempt()?.studentId === this.auth.user()?.id &&
      this.attempt()?.status === 1 &&
      this.examsData.current()?.status === 2 &&
      this.remaining() > 0 &&
      this.now() < new Date(this.examsData.current()!.deadline).getTime(),
  );
  protected readonly editable = computed(
    () =>
      this.active() &&
      !!this.question() &&
      !this.submission() &&
      !this.saving() &&
      !this.syncRequired() &&
      !this.questionLoading(),
  );
  protected readonly languageName = computed(
    () => ({ 1: 'cpp', 2: 'java', 3: 'python' })[this.language()],
  );
  protected readonly clock = computed(() => {
    const seconds = this.remaining();
    return [Math.floor(seconds / 3600), Math.floor(seconds / 60) % 60, seconds % 60]
      .map((value) => value.toString().padStart(2, '0'))
      .join(':');
  });

  private readonly formBuilder = inject(FormBuilder).nonNullable;

  protected readonly form = this.formBuilder.group({
    score: this.formBuilder.control<number | null>(null, [Validators.required, Validators.min(0)]),
    feedback: ['', [Validators.maxLength(8000)]],
  });

  constructor() {
    const timer = setInterval(() => this.now.set(Date.now()), 1000);
    this.destroy.onDestroy(() => {
      clearInterval(timer);
      this.selectionCancelled.next();
      this.selectionCancelled.complete();
    });
    effect(() => {
      const question = this.question();
      const submission = this.submission();
      this.form.controls.score.setValidators([
        Validators.required,
        Validators.min(0),
        Validators.max(question?.maxScore ?? 0),
      ]);
      this.form.reset({ score: submission?.score ?? null, feedback: submission?.feedback ?? '' });
    });
  }

  ngOnInit(): void {
    const selected =
      this.questions().find((item) => item.id === this.selectedQuestion()) ?? this.questions()[0];
    if (selected) this.select(selected.id);
  }

  select(id: string): void {
    this.selectionCancelled.next();
    this.selectedQuestion.set(id);
    this.questionLoading.set(true);
    this.questionError.set('');
    this.question.set(null);
    this.submission.set(null);
    forkJoin({
      question: this.questionsData.get(id),
      submission: this.submissionsData.forQuestion(this.attempt()!.id, id),
    })
      .pipe(
        takeUntil(this.selectionCancelled),
        tap((data) => {
          this.question.set(data.question);
          this.submission.set(data.submission);
          const draft = this.drafts().get(id);
          this.code.set(data.submission?.submittedCode ?? draft?.code ?? '');
          this.language.set(
            data.submission?.language ?? draft?.language ?? ProgrammingLanguage.Cpp,
          );
        }),
        tap({ error: (error) => this.questionError.set(apiError(error)) }),
        catchErrorToast('Could not load this question.', this.toast),
        finalize(() => this.questionLoading.set(false)),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe();
  }

  moreQuestions(): void {
    this.questionsData
      .more(this.examsData.current()!.id)
      .pipe(
        catchErrorToast('Could not load more questions.', this.toast),
        takeUntilDestroyed(this.destroy),
      )
      .subscribe();
  }

  submitted(questionId: string) {
    return this.submissions().find((item) => item.questionId === questionId);
  }

  edit(code: string): void {
    if (!this.editable()) return;
    this.code.set(code);
    this.storeDraft();
  }

  changeLanguage(language: number): void {
    if (!this.editable()) return;
    this.language.set(language);
    this.storeDraft();
  }

  submit(): void {
    if (!this.editable() || !this.code().trim() || this.code().length > 100000) return;
    const questionId = this.question()!.id;
    this.submissionsData
      .submit({
        attemptId: this.attempt()!.id,
        questionId,
        submittedCode: this.code(),
        language: this.language(),
      })
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe(() => {
        this.drafts.update((drafts) => {
          const next = new Map(drafts);
          next.delete(questionId);
          return next;
        });
        this.code.set(this.submission()?.submittedCode ?? this.code());
        this.toast.success('Code submitted.');
        this.confirm.set(null);
      });
  }

  finish(): void {
    const attempt = this.attempt();
    if (!attempt || !this.active() || this.saving() || this.syncRequired()) return;
    this.attemptsData
      .finish(attempt.id, attempt.version)
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe(() => {
        this.drafts.set(new Map());
        this.code.set(this.submission()?.submittedCode ?? '');
        this.toast.success('Attempt finished.');
        this.confirm.set(null);
      });
  }

  grade(): void {
    this.form.markAllAsTouched();
    const submission = this.submission();
    if (!this.manager() || !submission || this.form.invalid || this.saving() || this.syncRequired())
      return;
    const value = this.form.getRawValue();
    this.submissionsData
      .grade(submission.id, {
        version: submission.version,
        score: value.score!,
        feedback: value.feedback.trim() || null,
      })
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe(() => this.toast.success('Mark saved.'));
  }

  confirmAction(): void {
    if (this.confirm() === 'submit') this.submit();
    else this.finish();
  }

  download(): void {
    const extension = { 1: 'cpp', 2: 'java', 3: 'py' }[this.language()];
    const url = URL.createObjectURL(new Blob([this.code()], { type: 'text/plain;charset=utf-8' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = 'question-' + ((this.question()?.displayOrder ?? 0) + 1) + '.' + extension;
    link.click();
    URL.revokeObjectURL(url);
  }

  private storeDraft(): void {
    const id = this.question()?.id;
    if (id)
      this.drafts.update((drafts) =>
        new Map(drafts).set(id, { code: this.code(), language: this.language() }),
      );
  }
}
