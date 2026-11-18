import { UpdateLectureDTO } from '../../DTO/lectures/update-lecture.dto';
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Urls } from '../../constants/urls';
import { LectureListDTO } from '../../DTO/lectures/lecture-list.dto';
import { LectureDetailDTO } from '../../DTO/lectures/lecture-detail.dto';
import { PageDTO } from '../../DTO/common/page.dto';
import { CreateLectureDTO } from '../../DTO/lectures/create-lecture.dto';
import { CreatedDTO } from '../../DTO/common/created.dto';

@Injectable({ providedIn: 'root' })
export class LecturesApiService {
  private readonly http = inject(HttpClient);
  private readonly url = `${Urls.api}/lectures`;

  list(page = 1, filters: Record<string, string | undefined> = {}) {
    const params: Record<string, string | number> = { Page: page, RecordsPerPage: 20 };
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined) params[key] = value;
    }

    return this.http.get<PageDTO<LectureListDTO>>(this.url, { params });
  }

  get(id: string) {
    return this.http.get<LectureDetailDTO>(`${this.url}/${id}`);
  }

  create(request: CreateLectureDTO) {
    return this.http.post<CreatedDTO>(this.url, request);
  }

  start(id: string, version: string) {
    return this.http.post<void>(`${this.url}/${id}/start`, { version });
  }

  end(id: string, version: string) {
    return this.http.post<void>(`${this.url}/${id}/end`, { version });
  }
  update(id: string, request: UpdateLectureDTO) {
    return this.http.put<void>(`${this.url}/${id}`, request);
  }

  remove(id: string, version: string) {
    return this.http.delete<void>(`${this.url}/${id}`, { params: { version } });
  }
}
