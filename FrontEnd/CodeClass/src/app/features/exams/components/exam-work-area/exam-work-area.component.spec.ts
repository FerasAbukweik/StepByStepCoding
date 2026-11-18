import { HttpErrorResponse } from '@angular/common/http';
import { Component, input, output, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { Subject, of, throwError } from 'rxjs';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ExamWorkAreaComponent } from './exam-work-area.component';
import { ExamDetailComponent } from '../../detail/exam-detail.component';
import { CodeEditorComponent } from '../../../../shared/components/code-editor/code-editor.component';
import { EditorChange } from '../../../../shared/components/code-editor/editor-change.model';
import { ExamsService } from '../../../../core/services/data/exams-service';
import { ExamQuestionsService } from '../../../../core/services/data/exam-questions-service';
import { AttemptsService } from '../../../../core/services/data/attempts-service';
import { SubmissionsService } from '../../../../core/services/data/submissions-service';
import { AuthService } from '../../../../core/services/data/auth-service';
import { ExamsApiService } from '../../../../core/services/api/exams-api-service';
import { ExamQuestionsApiService } from '../../../../core/services/api/exam-questions-api-service';
import { AttemptsApiService } from '../../../../core/services/api/attempts-api-service';
import { SubmissionsApiService } from '../../../../core/services/api/submissions-api-service';
import { SectionsApiService } from '../../../../core/services/api/sections-api-service';
import { ToastService } from '../../../../core/services/local/toast-service';
import { SubmissionDetailDTO } from '../../../../core/DTO/submissions/submission-detail.dto';

@Component({ selector: 'app-code-editor', template: '' })
class EditorStub {
  readonly code = input('');
  readonly language = input('cpp');
  readonly readOnly = input(false);
  readonly changed = output<EditorChange>();
}

describe('Exam work area', () => {
  let fixture: ComponentFixture<ExamWorkAreaComponent>;
  let component: ExamWorkAreaComponent;
  let api: ReturnType<typeof createApi>;
  let stored: SubmissionDetailDTO | null;
  let finished: boolean;
  const roles = signal(['Student']);
  const user = signal({ id: 'student', fullName: 'Student' });
  const now = '2026-11-17T10:00:00Z';
  const attempt = {
    id: 'attempt',
    version: 'attempt-v1',
    examId: 'exam',
    studentId: 'student',
    studentName: 'Student',
    startedAt: now,
    expiresAt: '2026-11-17T11:00:00Z',
    status: 1,
  };
  const exam = {
    id: 'exam',
    instructorId: 'teacher',
    sectionId: 'section',
    version: 'exam-v1',
    title: 'Exam',
    status: 2,
    startsAt: '2026-11-17T09:00:00Z',
    deadline: '2026-11-17T12:00:00Z',
    durationMinutes: 60,
    instructions: null,
  };
  const questions = [1, 2].map((number) => ({
    id: 'q' + number,
    version: 'v1',
    examId: 'exam',
    title: 'Question ' + number,
    description: 'Write code.',
    imagePath: null,
    maxScore: 10,
    displayOrder: number,
  }));
  const page = <T>(items: T[]) => ({ items, totalCount: items.length });
  const toast = { success: vi.fn(), error: vi.fn() };

  function createApi() {
    return {
      questions: vi.fn(() => of(page(questions))),
      question: vi.fn((id: string) => of(questions.find((item) => item.id === id)!)),
      list: vi.fn(() => of(page([attempt]))),
      get: vi.fn(() =>
        of({
          ...attempt,
          status: finished ? 2 : 1,
          version: finished ? 'attempt-v3' : stored ? 'attempt-v2' : 'attempt-v1',
        }),
      ),
      submissions: vi.fn(() => of(page(stored ? [stored] : []))),
      submission: vi.fn(() => of(stored!)),
      submit: vi.fn((request) => {
        stored = {
          id: 'saved',
          version: 'saved-v1',
          attemptId: request.attemptId,
          questionId: request.questionId,
          submittedCode: request.submittedCode,
          language: request.language,
          submittedAt: now,
          score: null,
          feedback: null,
          reviewedBy: null,
          reviewedAt: null,
        };
        return of({ id: 'saved' });
      }),
      start: vi.fn(() => of({ id: 'attempt' })),
      finish: vi.fn(() => {
        finished = true;
        return of(undefined);
      }),
      grade: vi.fn((_id: string, request: { score: number; feedback: string | null }) => {
        stored = {
          ...stored!,
          score: request.score,
          feedback: request.feedback,
          version: 'saved-v2',
        };
        return of(undefined);
      }),
    };
  }

  beforeEach(async () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(now));
    stored = null;
    finished = false;
    roles.set(['Student']);
    user.set({ id: 'student', fullName: 'Student' });
    api = createApi();
    await TestBed.configureTestingModule({
      imports: [ExamWorkAreaComponent],
      providers: [
        { provide: ExamsApiService, useValue: { get: () => of(exam) } },
        { provide: ExamQuestionsApiService, useValue: { list: api.questions, get: api.question } },
        {
          provide: AttemptsApiService,
          useValue: { list: api.list, get: api.get, start: api.start, finish: api.finish },
        },
        {
          provide: SubmissionsApiService,
          useValue: {
            list: api.submissions,
            get: api.submission,
            submit: api.submit,
            grade: api.grade,
          },
        },
        { provide: SectionsApiService, useValue: {} },
        {
          provide: AuthService,
          useValue: { user, hasRole: (role: string) => roles().includes(role) },
        },
        { provide: ToastService, useValue: toast },
        { provide: Router, useValue: { navigateByUrl: vi.fn() } },
      ],
    })
      .overrideComponent(ExamWorkAreaComponent, {
        remove: { imports: [CodeEditorComponent] },
        add: { imports: [EditorStub] },
      })
      .overrideComponent(ExamDetailComponent, { set: { template: '', imports: [], providers: [] } })
      .compileComponents();
    TestBed.inject(ExamsService).current.set(exam);
    TestBed.inject(ExamQuestionsService).items.set(new Map([[1, questions]]));
    TestBed.inject(AttemptsService).current.set(attempt);
    fixture = TestBed.createComponent(ExamWorkAreaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    fixture.destroy();
    TestBed.resetTestingModule();
    vi.useRealTimers();
    vi.clearAllMocks();
  });

  function editor(): EditorStub {
    return fixture.debugElement.query(By.directive(EditorStub)).componentInstance;
  }

  it('keeps separate drafts and languages while switching questions', () => {
    component.edit('draft one');
    component.changeLanguage(3);
    component.select('q2');
    component.edit('draft two');
    component.select('q1');
    fixture.detectChanges();
    expect(editor().code()).toBe('draft one');
    expect(editor().language()).toBe('python');
    expect(component.drafts().get('q2')?.code).toBe('draft two');
    expect(api.submit).not.toHaveBeenCalled();
  });

  it('restores the parent-held drafts when the work area is recreated after refresh', () => {
    component.edit('keep this draft');
    const drafts = component.drafts();
    fixture.destroy();
    fixture = TestBed.createComponent(ExamWorkAreaComponent);
    fixture.componentRef.setInput('drafts', drafts);
    fixture.componentRef.setInput('selectedQuestion', 'q1');
    component = fixture.componentInstance;
    fixture.detectChanges();
    expect(editor().code()).toBe('keep this draft');
  });

  it('submits once and the submission service updates the attempt version and locks the code', () => {
    component.edit('print(42)');
    component.changeLanguage(3);
    component.submit();
    fixture.detectChanges();
    expect(api.submit).toHaveBeenCalledWith({
      attemptId: 'attempt',
      questionId: 'q1',
      submittedCode: 'print(42)',
      language: 3,
    });
    expect(TestBed.inject(AttemptsService).current()?.version).toBe('attempt-v2');
    expect(editor().readOnly()).toBe(true);
    expect(component.drafts().size).toBe(0);
    component.edit('cannot change submitted code');
    component.submit();
    fixture.detectChanges();
    expect(editor().code()).toBe('print(42)');
    expect(api.submit).toHaveBeenCalledOnce();
  });

  it('blocks another submission while the saved attempt snapshot is pending', () => {
    component.edit('draft');
    const snapshot = new Subject<
      ReturnType<typeof api.get> extends import('rxjs').Observable<infer T> ? T : never
    >();
    api.get.mockReturnValue(snapshot);
    component.submit();
    fixture.detectChanges();
    expect(TestBed.inject(SubmissionsService).saving()).toBe(true);
    expect(editor().readOnly()).toBe(true);
    component.submit();
    expect(api.submit).toHaveBeenCalledOnce();
    snapshot.next({ ...attempt, version: 'attempt-v2' });
    snapshot.complete();
    expect(TestBed.inject(SubmissionsService).saving()).toBe(false);
  });

  it('preserves failed submission drafts and does not show a success toast', () => {
    component.edit('draft');
    api.submit.mockImplementation(() =>
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: { message: 'Attempt changed. Refresh and retry.' },
          }),
      ),
    );
    component.submit();
    fixture.detectChanges();
    expect(component.drafts().get('q1')?.code).toBe('draft');
    expect(editor().code()).toBe('draft');
    expect(editor().readOnly()).toBe(false);
    expect(toast.success).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Attempt changed');
  });

  it('requires refresh if the post succeeded but the updated snapshot could not be fetched', () => {
    component.edit('already persisted');
    api.get.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 503 })));
    component.submit();
    fixture.detectChanges();
    expect(TestBed.inject(SubmissionsService).refreshRequired()).toBe(true);
    expect(editor().readOnly()).toBe(true);
    component.submit();
    expect(api.submit).toHaveBeenCalledOnce();
    expect(fixture.nativeElement.textContent).toContain('Refresh the exam');
  });

  it('becomes read only at expiry without submitting or clearing drafts', () => {
    component.edit('unsent');
    vi.advanceTimersByTime(3600000);
    fixture.detectChanges();
    expect(editor().readOnly()).toBe(true);
    expect(component.drafts().get('q1')?.code).toBe('unsent');
    expect(api.submit).not.toHaveBeenCalled();
  });

  it('finishes using the latest version after submitting a question', () => {
    component.edit('saved code');
    component.submit();
    component.select('q2');
    component.edit('unsubmitted draft');
    component.finish();
    fixture.detectChanges();
    expect(api.finish).toHaveBeenCalledWith('attempt', 'attempt-v2');
    expect(TestBed.inject(AttemptsService).current()?.status).toBe(2);
    expect(component.drafts().size).toBe(0);
    expect(editor().readOnly()).toBe(true);
  });

  it('keeps an awarded zero distinct from pending grading and refreshes the saved mark', () => {
    fixture.componentRef.setInput('manager', true);
    stored = {
      id: 'saved',
      version: 'saved-v1',
      attemptId: 'attempt',
      questionId: 'q1',
      submittedCode: 'code',
      language: 1,
      submittedAt: now,
      score: null,
      feedback: null,
      reviewedBy: null,
      reviewedAt: null,
    };
    TestBed.inject(SubmissionsService).items.set(new Map([[1, [stored]]]));
    component.select('q1');
    fixture.detectChanges();
    const score = fixture.nativeElement.querySelector('input[type="number"]') as HTMLInputElement;
    score.value = '0';
    score.dispatchEvent(new Event('input'));
    const feedback = fixture.nativeElement.querySelector('textarea') as HTMLTextAreaElement;
    feedback.value = ' Needs work ';
    feedback.dispatchEvent(new Event('input'));
    component.grade();
    fixture.detectChanges();
    expect(api.grade).toHaveBeenCalledWith('saved', {
      version: 'saved-v1',
      score: 0,
      feedback: 'Needs work',
    });
    expect(TestBed.inject(SubmissionsService).current()?.score).toBe(0);
    expect(fixture.nativeElement.textContent.replace(/\s+/g, ' ')).toContain('1 graded');
  });

  it('does not give management actions to an instructor enrolled in another instructor’s exam', () => {
    roles.set(['Instructor', 'Student']);
    const parent = TestBed.createComponent(ExamDetailComponent);
    parent.componentRef.setInput('examId', 'exam');
    parent.detectChanges();
    const state = parent.componentInstance as unknown as { manager: () => boolean };
    expect(state.manager()).toBe(false);
    parent.destroy();
  });
});
