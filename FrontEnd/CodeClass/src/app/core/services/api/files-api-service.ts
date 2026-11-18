import { RunFileDTO } from '../../DTO/files/run-file.dto';
import { CodeExecutionResultDTO } from '../../DTO/execution/code-execution-result.dto';
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Urls } from '../../constants/urls';
import { LectureFileListDTO } from '../../DTO/files/lecture-file-list.dto';
import { FileDetailDTO } from '../../DTO/files/file-detail.dto';
import { PageDTO } from '../../DTO/common/page.dto';
import { CreateLectureFileDTO } from '../../DTO/lecturefiles/create-lecture-file.dto';
import { CreatedDTO } from '../../DTO/common/created.dto';

@Injectable({ providedIn: 'root' })
export class FilesApiService {
  private readonly http = inject(HttpClient);
  private readonly url = `${Urls.api}/files`;

  list(page = 1, filters: Record<string, string | undefined> = {}) {
    const params: Record<string, string | number> = { Page: page, RecordsPerPage: 20 };
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined) params[key] = value;
    }

    return this.http.get<PageDTO<LectureFileListDTO>>(this.url, { params });
  }

  get(id: string) {
    return this.http.get<FileDetailDTO>(`${this.url}/${id}`);
  }

  run(id: string, request: RunFileDTO) {
    return this.http.post<CodeExecutionResultDTO>(`${this.url}/${id}/run`, request);
  }

  create(request: CreateLectureFileDTO) {
    return this.http.post<CreatedDTO>(this.url, request);
  }
}
