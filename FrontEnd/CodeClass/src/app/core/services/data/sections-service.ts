import { Injectable, inject } from '@angular/core';
import { SectionsApiService } from '../api/sections-api-service';
import { ResourceState } from '../../Utilities/resource-state';
import { SectionListDTO } from '../../DTO/sections/section-list.dto';
import { SectionDetailDTO } from '../../DTO/sections/section-detail.dto';
import { CreateSectionDTO } from '../../DTO/sections/create-section.dto';
import { UpdateSectionDTO } from '../../DTO/sections/update-section.dto';

@Injectable({ providedIn: 'root' })
export class SectionsService extends ResourceState<SectionListDTO, SectionDetailDTO> {
  private readonly api = inject(SectionsApiService);

  list(page = 1, filters: Record<string, string | undefined> = {}) {
    return this.listResult((filters) => this.api.list(page, filters), page, filters);
  }

  load(page = 1, filters: Record<string, string | undefined> = {}) {
    return this.loadPage(this.list(page, filters), page);
  }

  more(filters: Record<string, string | undefined> = {}) {
    return this.nextPage((page) => this.load(page, filters));
  }

  get(id: string) {
    return this.detail(this.api.get(id));
  }

  create(request: CreateSectionDTO) {
    return this.write(this.api.create(request), 'Could not create this section.');
  }

  update(id: string, request: UpdateSectionDTO) {
    return this.write(this.api.update(id, request), 'Could not update this section.');
  }

  remove(id: string, version: string) {
    return this.write(this.api.remove(id, version), 'Could not delete this section.');
  }

  courseName(id: string): string {
    return this.find(id)?.courseName || 'Course section';
  }

  sectionName(id: string): string {
    return this.find(id)?.name || 'Section';
  }

  instructorName(id: string): string {
    return this.find(id)?.instructorName || 'Instructor';
  }
}
