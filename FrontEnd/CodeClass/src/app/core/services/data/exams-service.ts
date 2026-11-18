import { map, switchMap, tap } from 'rxjs';
import { Injectable, inject } from '@angular/core';
import { ExamsApiService } from '../api/exams-api-service';
import { ResourceState } from '../../Utilities/resource-state';
import { ExamListDTO } from '../../DTO/exams/exam-list.dto';
import { ExamDetailDTO } from '../../DTO/exams/exam-detail.dto';
import { CreateExamDTO } from '../../DTO/exams/create-exam.dto';
import { UpdateExamDTO } from '../../DTO/exams/update-exam.dto';

@Injectable({ providedIn: 'root' })
export class ExamsService extends ResourceState<ExamListDTO, ExamDetailDTO> {
  private readonly api = inject(ExamsApiService);

  list(page = 1, filters: Record<string, string | undefined> = {}) {
    return this.listResult((filters) => this.api.list(page, filters), page, filters);
  }

  load(page = 1, filters: Record<string, string | undefined> = {}) {
    return this.loadPage(this.list(page, filters), page);
  }

  more(filters: Record<string, string | undefined> = {}) {
    return this.nextPage((page) => this.load(page, filters));
  }

  get(id: string) {
    return this.detail(this.api.get(id));
  }

  refresh(id: string) {
    return this.get(id).pipe(
      tap((exam) => {
        if (!this.current() || this.current()?.id === id) this.current.set(exam);
        this.updateItems((items) => items.map((item) => (item.id === id ? exam : item)));
      }),
    );
  }

  create(request: CreateExamDTO) {
    return this.write(this.api.create(request), 'Could not create this exam.');
  }

  update(id: string, request: UpdateExamDTO) {
    return this.write(
      this.api.update(id, request).pipe(
        switchMap(() => this.refresh(id)),
        map(() => undefined),
      ),
      'Could not update this exam.',
    );
  }

  remove(id: string, version: string) {
    return this.write(
      this.api.remove(id, version).pipe(
        tap(() => {
          this.updateItems((items) => items.filter((item) => item.id !== id));
          if (this.current()?.id === id) this.current.set(null);
          this.total.update((total) => Math.max(0, total - 1));
        }),
      ),
      'Could not delete this exam.',
    );
  }

  publish(id: string, version: string) {
    return this.write(
      this.api.publish(id, version).pipe(
        switchMap(() => this.refresh(id)),
        map(() => undefined),
      ),
      'Could not publish this exam.',
    );
  }

  close(id: string, version: string) {
    return this.write(
      this.api.close(id, version).pipe(
        switchMap(() => this.refresh(id)),
        map(() => undefined),
      ),
      'Could not close this exam.',
    );
  }
}
