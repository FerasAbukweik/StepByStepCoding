import { Injectable, inject, signal } from '@angular/core';
import { forkJoin, map, of, switchMap, tap } from 'rxjs';
import { SubmissionsApiService } from '../api/submissions-api-service';
import { AttemptsService } from './attempts-service';
import { ResourceState } from '../../Utilities/resource-state';
import { SubmissionListDTO } from '../../DTO/submissions/submission-list.dto';
import { SubmissionDetailDTO } from '../../DTO/submissions/submission-detail.dto';
import { SubmitCodeDTO } from '../../DTO/submissions/submit-code.dto';
import { GradeSubmissionDTO } from '../../DTO/submissions/grade-submission.dto';

@Injectable({ providedIn: 'root' })
export class SubmissionsService extends ResourceState<SubmissionListDTO, SubmissionDetailDTO> {
  private readonly api = inject(SubmissionsApiService);
  private readonly attempts = inject(AttemptsService);
  readonly refreshRequired = signal(false);

  list(attemptId: string, page = 1) {
    return this.listResult((filters) => this.api.list(filters.attemptId, page), page, {
      attemptId,
    });
  }

  load(attemptId: string, page = 1) {
    return this.loadPage(this.list(attemptId, page), page);
  }

  forQuestion(attemptId: string, questionId: string) {
    return this.listResult(
      (filters) => this.api.list(filters.attemptId, 1, filters.questionId),
      1,
      {
        attemptId,
        questionId,
      },
    ).pipe(
      switchMap((result) => {
        const submission = result.items.find((item) => item.questionId === questionId);
        return submission ? this.get(submission.id) : of(null);
      }),
      tap((submission) => {
        if (submission) this.accept(submission);
      }),
    );
  }

  get(id: string) {
    return this.detail(this.api.get(id));
  }

  submit(request: SubmitCodeDTO) {
    return this.write(
      this.api.submit(request).pipe(
        tap(() => this.refreshRequired.set(true)),
        switchMap((created) =>
          forkJoin({
            submission: this.get(created.id),
            attempt: this.attempts.refresh(request.attemptId),
          }).pipe(
            // Submission advances the attempt version used by Finish.
            tap(({ submission }) => this.accept(submission)),
            map(() => created),
          ),
        ),
      ),
      'Could not submit this answer.',
    );
  }

  grade(id: string, request: GradeSubmissionDTO) {
    return this.write(
      this.api.grade(id, request).pipe(
        tap(() => this.refreshRequired.set(true)),
        switchMap(() => this.get(id)),
        tap((submission) => this.accept(submission)),
        map(() => undefined),
      ),
      'Could not save this mark.',
    );
  }

  override reset(): void {
    super.reset();
    this.refreshRequired.set(false);
  }

  private accept(submission: SubmissionDetailDTO): void {
    this.updateItems((items) =>
      items.map((item) => (item.id === submission.id ? submission : item)),
    );
    if (!this.find(submission.id)) {
      this.items.set(new Map([[1, [submission]]]));
      this.total.set(1);
    }
    this.current.set(submission);
    this.refreshRequired.set(false);
  }
}
