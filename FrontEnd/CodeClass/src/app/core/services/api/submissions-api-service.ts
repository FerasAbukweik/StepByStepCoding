import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { CreatedDTO } from '../../DTO/common/created.dto';
import { SubmissionListDTO } from '../../DTO/submissions/submission-list.dto';
import { SubmissionDetailDTO } from '../../DTO/submissions/submission-detail.dto';
import { SubmitCodeDTO } from '../../DTO/submissions/submit-code.dto';
import { GradeSubmissionDTO } from '../../DTO/submissions/grade-submission.dto';
import { PageDTO } from '../../DTO/common/page.dto';

@Injectable({ providedIn: 'root' })
export class SubmissionsApiService {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/submissions';

  list(attemptId: string, page = 1, questionId?: string) {
    return this.http.get<PageDTO<SubmissionListDTO>>(this.url, {
      params: { Page: page, RecordsPerPage: 20, attemptId, ...(questionId ? { questionId } : {}) },
    });
  }

  get(id: string) {
    return this.http.get<SubmissionDetailDTO>(`${this.url}/${id}`);
  }

  submit(request: SubmitCodeDTO) {
    return this.http.post<CreatedDTO>(this.url, request);
  }

  grade(id: string, request: GradeSubmissionDTO) {
    return this.http.post<void>(`${this.url}/${id}/grade`, request);
  }
}
