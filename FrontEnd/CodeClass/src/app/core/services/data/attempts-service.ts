import { Injectable, inject, signal } from '@angular/core';
import { map, switchMap, tap } from 'rxjs';
import { AttemptsApiService } from '../api/attempts-api-service';
import { ResourceState } from '../../Utilities/resource-state';
import { ExamAttemptListDTO } from '../../DTO/examattempts/exam-attempt-list.dto';
import { ExamAttemptDetailDTO } from '../../DTO/examattempts/exam-attempt-detail.dto';

@Injectable({ providedIn: 'root' })
export class AttemptsService extends ResourceState<ExamAttemptListDTO, ExamAttemptDetailDTO> {
  private readonly api = inject(AttemptsApiService);
  readonly refreshRequired = signal(false);

  list(examId?: string, page = 1) {
    return this.listResult((filters) => this.api.list(filters.examId, page), page, { examId });
  }

  load(examId?: string, page = 1) {
    return this.loadPage(this.list(examId, page), page);
  }

  get(id: string) {
    return this.detail(this.api.get(id));
  }

  refresh(id: string) {
    return this.get(id).pipe(tap((attempt) => this.accept(attempt)));
  }

  start(examId: string) {
    return this.write(
      this.api.start(examId).pipe(
        tap(() => this.refreshRequired.set(true)),
        switchMap((created) => this.refresh(created.id).pipe(map(() => created))),
      ),
      'Could not start this attempt.',
    );
  }

  finish(id: string, version: string) {
    return this.write(
      this.api.finish(id, version).pipe(
        tap(() => this.refreshRequired.set(true)),
        switchMap(() => this.refresh(id)),
        map(() => undefined),
      ),
      'Could not finish this attempt.',
    );
  }

  override reset(): void {
    super.reset();
    this.refreshRequired.set(false);
  }

  private accept(attempt: ExamAttemptDetailDTO): void {
    this.current.set(attempt);
    this.refreshRequired.set(false);
    this.updateItems((items) =>
      items.map((item) => (item.id === attempt.id ? { ...item, ...attempt } : item)),
    );
  }
}
