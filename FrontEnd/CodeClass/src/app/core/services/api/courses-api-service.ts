import { CreateCourseDTO } from '../../DTO/courses/create-course.dto';
import { UpdateCourseDTO } from '../../DTO/courses/update-course.dto';
import { CreatedDTO } from '../../DTO/common/created.dto';
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Urls } from '../../constants/urls';
import { CourseListDTO } from '../../DTO/courses/course-list.dto';
import { CourseDetailDTO } from '../../DTO/courses/course-detail.dto';
import { PageDTO } from '../../DTO/common/page.dto';

@Injectable({ providedIn: 'root' })
export class CoursesApiService {
  private readonly http = inject(HttpClient);
  private readonly url = `${Urls.api}/courses`;

  list(page = 1, filters: Record<string, string | undefined> = {}) {
    const params: Record<string, string | number> = { Page: page, RecordsPerPage: 20 };
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined) params[key] = value;
    }

    return this.http.get<PageDTO<CourseListDTO>>(this.url, { params });
  }

  get(id: string) {
    return this.http.get<CourseDetailDTO>(`${this.url}/${id}`);
  }

  create(request: CreateCourseDTO) {
    return this.http.post<CreatedDTO>(this.url, request);
  }

  update(id: string, request: UpdateCourseDTO) {
    return this.http.put<void>(`${this.url}/${id}`, request);
  }

  remove(id: string, version: string) {
    return this.http.delete<void>(`${this.url}/${id}`, { params: { version } });
  }
}
