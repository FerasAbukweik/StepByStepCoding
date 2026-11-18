import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { CreatedDTO } from '../../DTO/common/created.dto';
import { ExamAttemptListDTO } from '../../DTO/examattempts/exam-attempt-list.dto';
import { ExamAttemptDetailDTO } from '../../DTO/examattempts/exam-attempt-detail.dto';
import { PageDTO } from '../../DTO/common/page.dto';

@Injectable({ providedIn: 'root' })
export class AttemptsApiService {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/examattempts';

  list(examId?: string, page = 1) {
    return this.http.get<PageDTO<ExamAttemptListDTO>>(this.url, {
      params: { Page: page, RecordsPerPage: 20, ...(examId ? { examId } : {}) },
    });
  }

  get(id: string) {
    return this.http.get<ExamAttemptDetailDTO>(`${this.url}/${id}`);
  }

  start(examId: string) {
    return this.http.post<CreatedDTO>(this.url, { examId });
  }

  finish(id: string, version: string) {
    return this.http.post<void>(`${this.url}/${id}/finish`, { version });
  }
}
