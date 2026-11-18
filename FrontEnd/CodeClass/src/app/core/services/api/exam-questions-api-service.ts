import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { CreatedDTO } from '../../DTO/common/created.dto';
import { ExamQuestionListDTO } from '../../DTO/examquestions/exam-question-list.dto';
import { ExamQuestionDetailDTO } from '../../DTO/examquestions/exam-question-detail.dto';
import { CreateExamQuestionDTO } from '../../DTO/examquestions/create-exam-question.dto';
import { UpdateExamQuestionDTO } from '../../DTO/examquestions/update-exam-question.dto';
import { PageDTO } from '../../DTO/common/page.dto';

@Injectable({ providedIn: 'root' })
export class ExamQuestionsApiService {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/examquestions';

  list(examId: string, page = 1) {
    return this.http.get<PageDTO<ExamQuestionListDTO>>(this.url, {
      params: { Page: page, RecordsPerPage: 20, examId },
    });
  }

  get(id: string) {
    return this.http.get<ExamQuestionDetailDTO>(`${this.url}/${id}`);
  }

  create(request: CreateExamQuestionDTO) {
    return this.http.post<CreatedDTO>(this.url, request);
  }

  update(id: string, request: UpdateExamQuestionDTO) {
    return this.http.put<void>(`${this.url}/${id}`, request);
  }

  remove(id: string, version: string) {
    return this.http.delete<void>(`${this.url}/${id}`, { params: { version } });
  }
}
