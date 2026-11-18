import { forkJoin, map, of, switchMap } from 'rxjs';
import { ExamsService } from './exams-service';
import { Injectable, inject } from '@angular/core';
import { ExamQuestionsApiService } from '../api/exam-questions-api-service';
import { ResourceState } from '../../Utilities/resource-state';
import { ExamQuestionListDTO } from '../../DTO/examquestions/exam-question-list.dto';
import { ExamQuestionDetailDTO } from '../../DTO/examquestions/exam-question-detail.dto';
import { CreateExamQuestionDTO } from '../../DTO/examquestions/create-exam-question.dto';
import { UpdateExamQuestionDTO } from '../../DTO/examquestions/update-exam-question.dto';

@Injectable({ providedIn: 'root' })
export class ExamQuestionsService extends ResourceState<
  ExamQuestionListDTO,
  ExamQuestionDetailDTO
> {
  private readonly exams = inject(ExamsService);
  private readonly api = inject(ExamQuestionsApiService);

  list(examId: string, page = 1) {
    return this.listResult((filters) => this.api.list(filters.examId, page), page, { examId });
  }

  load(examId: string, page = 1) {
    return this.loadPage(this.list(examId, page), page);
  }

  more(examId: string) {
    return this.nextPage((page) => this.load(examId, page));
  }

  get(id: string) {
    return this.detail(this.api.get(id));
  }

  create(request: CreateExamQuestionDTO) {
    return this.write(
      this.api
        .create(request)
        .pipe(switchMap((created) => this.refreshExam(request.examId).pipe(map(() => created)))),
      'Could not create this question.',
    );
  }

  update(id: string, request: UpdateExamQuestionDTO) {
    return this.write(
      this.parentId(id).pipe(
        switchMap((examId) =>
          this.api.update(id, request).pipe(switchMap(() => this.refreshExam(examId))),
        ),
        map(() => undefined),
      ),
      'Could not update this question.',
    );
  }

  remove(id: string, version: string) {
    return this.write(
      this.parentId(id).pipe(
        switchMap((examId) =>
          this.api.remove(id, version).pipe(switchMap(() => this.refreshExam(examId))),
        ),
        map(() => undefined),
      ),
      'Could not delete this question.',
    );
  }
  private parentId(id: string) {
    const question = this.find(id);
    return question ? of(question.examId) : this.get(id).pipe(map((item) => item.examId));
  }

  private refreshExam(examId: string) {
    return forkJoin([this.exams.refresh(examId), this.load(examId)]);
  }
}
