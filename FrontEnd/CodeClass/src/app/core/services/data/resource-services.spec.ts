import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { CoursesService } from './courses-service';
import { ExamQuestionsService } from './exam-questions-service';
import { SubmissionsService } from './submissions-service';
import { SectionsService } from './sections-service';
import { AuthService } from './auth-service';
import { ToastService } from '../local/toast-service';
import { memoryCacheInterceptor } from '../../interceptors/memory-cache-interceptor';

describe('Resource data services', () => {
  let courses: CoursesService;
  let sections: SectionsService;
  let requests: HttpTestingController;
  const user = signal({ id: 'first-user' });
  const toast = { error: vi.fn(), success: vi.fn() };

  beforeEach(() => {
    vi.useFakeTimers();
    user.set({ id: 'first-user' });
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([memoryCacheInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: { user } },
        { provide: ToastService, useValue: toast },
      ],
    });
    courses = TestBed.inject(CoursesService);
    sections = TestBed.inject(SectionsService);
    requests = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    requests.verify();
    TestBed.resetTestingModule();
    vi.useRealTimers();
    vi.clearAllMocks();
  });

  it('keeps course and section records, pagination and loading state separate', () => {
    courses.load(2).subscribe();
    sections.load(1, { courseId: 'course' }).subscribe();
    expect(courses.loading()).toBe(true);
    const courseRequest = requests.expectOne((request) => request.url === '/api/courses');
    expect(courseRequest.request.params.get('Page')).toBe('2');
    courseRequest.flush({
      items: [{ id: 'course', version: 'v1', name: 'Programming' }],
      totalCount: 25,
    });
    expect(courses.loading()).toBe(false);
    expect(sections.loading()).toBe(true);
    requests
      .expectOne((request) => request.url === '/api/sections')
      .flush({
        items: [
          {
            id: 'section',
            version: 'v1',
            courseId: 'course',
            name: 'A',
            courseName: 'Programming',
            instructorId: 'teacher',
            instructorName: 'Instructor',
          },
        ],
        totalCount: 1,
      });
    expect(courses.loadedItems()[0].name).toBe('Programming');
    expect(courses.items().get(2)?.[0].id).toBe('course');
    expect(courses.page()).toBe(2);
    expect(courses.total()).toBe(25);
    expect(sections.loadedItems()[0].name).toBe('A');
    expect(sections.courseName('section')).toBe('Programming');
  });

  it('loads one question page and only requests the next page explicitly', () => {
    const questions = TestBed.inject(ExamQuestionsService);
    questions.load('exam').subscribe();
    const first = requests.expectOne((request) => request.url === '/api/examquestions');
    expect(first.request.params.get('Page')).toBe('1');
    expect(first.request.params.get('RecordsPerPage')).toBe('20');
    first.flush({ items: [{ id: 'q1', examId: 'exam', title: 'First' }], totalCount: 25 });
    expect(questions.hasMore()).toBe(true);
    requests.expectNone((request) => request.url === '/api/examquestions');

    questions.more('exam').subscribe();
    const second = requests.expectOne((request) => request.url === '/api/examquestions');
    expect(second.request.params.get('Page')).toBe('2');
    expect(second.request.params.get('examId')).toBe('exam');
    second.flush({ items: [{ id: 'q2', examId: 'exam', title: 'Second' }], totalCount: 25 });
    expect([...questions.items().keys()]).toEqual([1, 2]);
    expect(questions.items().get(1)?.[0].id).toBe('q1');
    expect(questions.items().get(2)?.[0].id).toBe('q2');
    expect(questions.loadedItems().map((item) => item.id)).toEqual(['q1', 'q2']);
    requests.expectNone((request) => request.url === '/api/examquestions');
  });

  it('finds the selected question submission without downloading every submission page', () => {
    const submissions = TestBed.inject(SubmissionsService);
    const result = vi.fn();
    submissions.forQuestion('attempt', 'later-question').subscribe(result);
    const request = requests.expectOne((request) => request.url === '/api/submissions');
    expect(request.request.params.get('attemptId')).toBe('attempt');
    expect(request.request.params.get('questionId')).toBe('later-question');
    request.flush({ items: [{ id: 'submission', questionId: 'later-question' }], totalCount: 1 });
    requests
      .expectOne('/api/submissions/submission')
      .flush({ id: 'submission', questionId: 'later-question', score: 0 });
    expect(result).toHaveBeenCalledWith(expect.objectContaining({ id: 'submission', score: 0 }));
    requests.expectNone((request) => request.url === '/api/submissions');
  });

  it('returns a saved page without another request, including after the HTTP cache expires', () => {
    courses.load(1).subscribe();
    requests
      .expectOne((request) => request.url === '/api/courses')
      .flush({ items: [{ id: 'course-one', name: 'First page' }], totalCount: 25 });
    courses.load(2).subscribe();
    requests
      .expectOne((request) => request.url === '/api/courses')
      .flush({ items: [{ id: 'course-two', name: 'Second page' }], totalCount: 25 });
    vi.advanceTimersByTime(30001);
    const result = vi.fn();
    courses.load(1).subscribe(result);
    requests.expectNone((request) => request.url === '/api/courses');
    expect(result).toHaveBeenCalledWith({
      items: [{ id: 'course-one', name: 'First page' }],
      totalCount: 25,
    });
    expect(courses.loadedItems().map((item) => item.id)).toEqual(['course-one']);
    expect(courses.page()).toBe(1);

    courses.more().subscribe();
    requests.expectNone((request) => request.url === '/api/courses');
    expect(courses.loadedItems().map((item) => item.id)).toEqual(['course-one', 'course-two']);
  });

  it('does not reuse a page number for different filters', () => {
    sections.load(1, { courseId: 'first' }).subscribe();
    requests
      .expectOne((request) => request.url === '/api/sections')
      .flush({ items: [{ id: 'first-section' }], totalCount: 1 });
    sections.load(1, { courseId: 'second' }).subscribe();
    const request = requests.expectOne((request) => request.url === '/api/sections');
    expect(request.request.params.get('courseId')).toBe('second');
    request.flush({ items: [{ id: 'second-section' }], totalCount: 1 });
    expect(sections.loadedItems().map((item) => item.id)).toEqual(['second-section']);
  });

  it('invalidates saved pages after a successful mutation in another resource', () => {
    courses.load().subscribe();
    requests
      .expectOne((request) => request.url === '/api/courses')
      .flush({ items: [{ id: 'old-course' }], totalCount: 1 });
    sections
      .create({ courseId: 'course', name: 'New section', instructorId: 'teacher' })
      .subscribe();
    requests.expectOne((request) => request.url === '/api/sections').flush({ id: 'new-section' });
    courses.load().subscribe();
    requests
      .expectOne((request) => request.url === '/api/courses')
      .flush({ items: [{ id: 'updated-course' }], totalCount: 1 });
    expect(courses.loadedItems().map((item) => item.id)).toEqual(['updated-course']);
  });

  it('resets all state and requests page one when the cache version changes before loading more', () => {
    courses.load(2).subscribe();
    requests
      .expectOne((request) => request.url === '/api/courses')
      .flush({
        items: [{ id: 'old-course', name: 'Old page' }],
        totalCount: 25,
      });
    courses.current.set({ id: 'old-course', version: 'v1', name: 'Old detail' });
    courses.error.set('Old read error');
    courses.actionError.set('Old write error');
    sections
      .create({ courseId: 'course', name: 'New section', instructorId: 'teacher' })
      .subscribe();
    requests.expectOne((request) => request.url === '/api/sections').flush({ id: 'section' });

    courses.more().subscribe();
    const first = requests.expectOne((request) => request.url === '/api/courses');
    expect(first.request.params.get('Page')).toBe('1');
    expect(courses.page()).toBe(1);
    expect(courses.total()).toBe(0);
    expect(courses.items().size).toBe(0);
    expect(courses.current()).toBeNull();
    expect(courses.error()).toBe('');
    expect(courses.actionError()).toBe('');
    first.flush(null, { status: 503, statusText: 'Unavailable' });

    courses.more().subscribe();
    const retry = requests.expectOne((request) => request.url === '/api/courses');
    expect(retry.request.params.get('Page')).toBe('1');
    retry.flush({ items: [{ id: 'fresh-course' }], totalCount: 1 });
    expect([...courses.items().keys()]).toEqual([1]);
  });

  it('does not cancel a successful mutation when its follow-up page load resets the cache state', () => {
    const questions = TestBed.inject(ExamQuestionsService);
    questions.load('exam').subscribe();
    requests
      .expectOne((request) => request.url === '/api/examquestions')
      .flush({ items: [], totalCount: 0 });
    const saved = vi.fn();
    questions
      .create({
        examId: 'exam',
        title: 'New',
        description: 'Code',
        imagePath: null,
        maxScore: 10,
        displayOrder: 0,
      })
      .subscribe(saved);
    requests.expectOne((request) => request.url === '/api/examquestions').flush({ id: 'question' });
    requests.expectOne('/api/exams/exam').flush({ id: 'exam', version: 'v2' });
    requests
      .expectOne((request) => request.url === '/api/examquestions')
      .flush({ items: [{ id: 'question', examId: 'exam' }], totalCount: 1 });
    expect(saved).toHaveBeenCalledWith({ id: 'question' });
    expect(questions.saving()).toBe(false);
    expect(questions.items().get(1)?.[0].id).toBe('question');
  });

  it('keeps saved pages when a mutation fails', () => {
    courses.load().subscribe();
    requests
      .expectOne((request) => request.url === '/api/courses')
      .flush({ items: [{ id: 'course', name: 'Original' }], totalCount: 1 });
    courses.update('course', { version: 'v1', name: 'Rejected' }).subscribe();
    requests
      .expectOne('/api/courses/course')
      .flush({ message: 'Conflict' }, { status: 409, statusText: 'Conflict' });
    vi.advanceTimersByTime(30001);
    courses.load().subscribe();
    requests.expectNone((request) => request.url === '/api/courses');
    expect(courses.loadedItems()[0].name).toBe('Original');
  });

  it('retries a page after a failed request instead of caching the error', () => {
    courses.load().subscribe({ error: () => {} });
    requests
      .expectOne((request) => request.url === '/api/courses')
      .flush(null, { status: 503, statusText: 'Unavailable' });
    courses.load().subscribe();
    requests
      .expectOne((request) => request.url === '/api/courses')
      .flush({ items: [{ id: 'retried' }], totalCount: 1 });
    expect(courses.loadedItems()[0].id).toBe('retried');
  });

  it('uses the caching interceptor instead of keeping a second read cache', () => {
    courses.get('course').subscribe();
    requests
      .expectOne('/api/courses/course')
      .flush({ id: 'course', version: 'v1', name: 'Old name' });
    courses.get('course').subscribe();
    requests.expectNone('/api/courses/course');
    vi.advanceTimersByTime(30001);
    courses.get('course').subscribe();
    requests
      .expectOne('/api/courses/course')
      .flush({ id: 'course', version: 'v2', name: 'Updated name' });
  });

  it('reports a failed update without emitting a successful result', () => {
    const success = vi.fn();
    courses.update('course', { version: 'v1', name: 'Duplicate' }).subscribe(success);
    expect(courses.saving()).toBe(true);
    requests
      .expectOne('/api/courses/course')
      .flush({ message: 'Name already exists.' }, { status: 409, statusText: 'Conflict' });
    expect(success).not.toHaveBeenCalled();
    expect(courses.saving()).toBe(false);
    expect(courses.actionError()).toBe('Name already exists.');
    expect(toast.error).toHaveBeenCalledOnce();
  });

  it('cancels the old user’s pending reads without cancelling the new user’s first load', () => {
    courses.load().subscribe();
    const oldRequest = requests.expectOne((request) => request.url === '/api/courses');
    user.set({ id: 'second-user' });
    courses.load().subscribe();
    expect(oldRequest.cancelled).toBe(true);
    const newRequest = requests.expectOne((request) => request.url === '/api/courses');
    TestBed.tick();
    expect(newRequest.cancelled).toBe(false);
    newRequest.flush({
      items: [{ id: 'new-course', version: 'v1', name: 'New account course' }],
      totalCount: 1,
    });
    expect(courses.loadedItems().map((item) => item.id)).toEqual(['new-course']);
  });
});
