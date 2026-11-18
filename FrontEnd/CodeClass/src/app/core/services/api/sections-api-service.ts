import { CreateSectionDTO } from '../../DTO/sections/create-section.dto';
import { UpdateSectionDTO } from '../../DTO/sections/update-section.dto';
import { CreatedDTO } from '../../DTO/common/created.dto';
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Urls } from '../../constants/urls';
import { SectionListDTO } from '../../DTO/sections/section-list.dto';
import { SectionDetailDTO } from '../../DTO/sections/section-detail.dto';
import { PageDTO } from '../../DTO/common/page.dto';

@Injectable({ providedIn: 'root' })
export class SectionsApiService {
  private readonly http = inject(HttpClient);
  private readonly url = `${Urls.api}/sections`;

  list(page = 1, filters: Record<string, string | undefined> = {}) {
    const params: Record<string, string | number> = { Page: page, RecordsPerPage: 20 };
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined) params[key] = value;
    }

    return this.http.get<PageDTO<SectionListDTO>>(this.url, { params });
  }

  get(id: string) {
    return this.http.get<SectionDetailDTO>(`${this.url}/${id}`);
  }

  create(request: CreateSectionDTO) {
    return this.http.post<CreatedDTO>(this.url, request);
  }

  update(id: string, request: UpdateSectionDTO) {
    return this.http.put<void>(`${this.url}/${id}`, request);
  }

  remove(id: string, version: string) {
    return this.http.delete<void>(`${this.url}/${id}`, { params: { version } });
  }
}
