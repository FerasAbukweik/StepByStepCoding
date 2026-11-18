import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { EnrollmentListDTO } from '../../DTO/enrollments/enrollment-list.dto';
import { CreatedDTO } from '../../DTO/common/created.dto';
import { PageDTO } from '../../DTO/common/page.dto';

@Injectable({ providedIn: 'root' })
export class EnrollmentsApiService {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/enrollments';

  list(sectionId: string, page = 1) {
    return this.http.get<PageDTO<EnrollmentListDTO>>(this.url, {
      params: { Page: page, RecordsPerPage: 20, sectionId },
    });
  }

  create(sectionId: string, studentId: string) {
    return this.http.post<CreatedDTO>(this.url, { sectionId, studentId });
  }

  update(id: string, version: string, active: boolean) {
    return this.http.put<void>(`${this.url}/${id}`, { version, active });
  }

  remove(id: string, version: string) {
    return this.http.delete<void>(`${this.url}/${id}`, { params: { version } });
  }
}
