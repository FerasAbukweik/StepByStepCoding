import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { beforeEach, afterEach, describe, expect, it } from 'vitest';
import { memoryCacheInterceptor } from './memory-cache-interceptor';

describe('memoryCacheInterceptor', () => {
  let http: HttpClient;
  let requests: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([memoryCacheInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    requests = TestBed.inject(HttpTestingController);
  });

  afterEach(() => requests.verify());

  it('requests concurrent cache misses separately and caches completed responses', () => {
    const received: string[] = [];
    http
      .get<{ name: string }>('/api/courses?page=1')
      .subscribe((value) => received.push(value.name));
    http
      .get<{ name: string }>('/api/courses?page=1')
      .subscribe((value) => received.push(value.name));
    const pending = requests.match('/api/courses?page=1');
    expect(pending).toHaveLength(2);
    for (const request of pending) request.flush({ name: 'Programming' });
    http
      .get<{ name: string }>('/api/courses?page=1')
      .subscribe((value) => received.push(value.name));
    requests.expectNone('/api/courses?page=1');
    expect(received).toEqual(['Programming', 'Programming', 'Programming']);
  });

  it('retries failed and cancelled reads', () => {
    http.get('/api/files/1').subscribe({ error: () => {} });
    requests.expectOne('/api/files/1').flush('failed', { status: 500, statusText: 'Error' });
    const subscription = http.get('/api/files/1').subscribe();
    const cancelled = requests.expectOne('/api/files/1');
    subscription.unsubscribe();
    expect(cancelled.cancelled).toBe(true);
    http.get('/api/files/1').subscribe();
    requests.expectOne('/api/files/1').flush({ id: '1' });
  });

  it('does not repopulate the cache with reads started before a mutation', () => {
    http.get('/api/courses').subscribe();
    const stale = requests.expectOne('/api/courses');
    http.post('/api/courses', {}).subscribe();
    requests.expectOne('/api/courses').flush({});
    stale.flush([]);
    http.get('/api/courses').subscribe();
    requests.expectOne('/api/courses').flush([]);
  });

  it('always requests Me from the server', () => {
    http.get('/api/auth/me').subscribe();
    requests.expectOne('/api/auth/me').flush({ id: 'user' });
    http.get('/api/auth/me').subscribe();
    requests.expectOne('/api/auth/me').flush({ id: 'user' });
  });

  it('invalidates saved file reads after a successful POST', () => {
    http.get('/api/files/file').subscribe();
    requests.expectOne('/api/files/file').flush({ savedRevision: 0 });
    http.post('/api/files/file/run', { code: 'draft', stdin: '' }).subscribe();
    requests.expectOne('/api/files/file/run').flush({ run: { stdout: '42' } });
    http.get('/api/files/file').subscribe();
    requests.expectOne('/api/files/file').flush({ savedRevision: 0 });
  });

  it('invalidates cached reads after a successful mutation', () => {
    http.get('/api/courses').subscribe();
    requests.expectOne('/api/courses').flush([]);
    http.post('/api/courses', { name: 'New course' }).subscribe();
    requests.expectOne('/api/courses').flush({ id: 'new' });
    http.get('/api/courses').subscribe();
    requests.expectOne('/api/courses').flush([{ id: 'new' }]);
  });
  it('invalidates account lookups after creating a user through signup', () => {
    http.get('/api/users?role=Student').subscribe();
    requests.expectOne('/api/users?role=Student').flush([]);
    http.post('/api/auth/signup', {}).subscribe();
    requests.expectOne('/api/auth/signup').flush(null, { status: 201, statusText: 'Created' });
    http.get('/api/users?role=Student').subscribe();
    requests.expectOne('/api/users?role=Student').flush([{ id: 'new-user' }]);
  });
});
