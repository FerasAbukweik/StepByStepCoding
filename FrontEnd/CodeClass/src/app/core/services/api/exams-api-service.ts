import { CreateExamDTO } from '../../DTO/exams/create-exam.dto';
import { UpdateExamDTO } from '../../DTO/exams/update-exam.dto';
import { CreatedDTO } from '../../DTO/common/created.dto';
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Urls } from '../../constants/urls';
import { ExamListDTO } from '../../DTO/exams/exam-list.dto';
import { ExamDetailDTO } from '../../DTO/exams/exam-detail.dto';
import { PageDTO } from '../../DTO/common/page.dto';

@Injectable({ providedIn: 'root' })
export class ExamsApiService {
  private readonly http = inject(HttpClient);
  private readonly url = `${Urls.api}/exams`;

  list(page = 1, filters: Record<string, string | undefined> = {}) {
    const params: Record<string, string | number> = { Page: page, RecordsPerPage: 20 };
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined) params[key] = value;
    }

    return this.http.get<PageDTO<ExamListDTO>>(this.url, { params });
  }

  get(id: string) {
    return this.http.get<ExamDetailDTO>(`${this.url}/${id}`);
  }

  create(request: CreateExamDTO) {
    return this.http.post<CreatedDTO>(this.url, request);
  }

  update(id: string, request: UpdateExamDTO) {
    return this.http.put<void>(`${this.url}/${id}`, request);
  }

  publish(id: string, version: string) {
    return this.http.post<void>(`${this.url}/${id}/publish`, { version });
  }

  close(id: string, version: string) {
    return this.http.post<void>(`${this.url}/${id}/close`, { version });
  }
  remove(id: string, version: string) {
    return this.http.delete<void>(`${this.url}/${id}`, { params: { version } });
  }
}
