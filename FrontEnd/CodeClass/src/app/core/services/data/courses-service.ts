import { Injectable, inject } from '@angular/core';
import { CoursesApiService } from '../api/courses-api-service';
import { ResourceState } from '../../Utilities/resource-state';
import { CourseListDTO } from '../../DTO/courses/course-list.dto';
import { CourseDetailDTO } from '../../DTO/courses/course-detail.dto';
import { CreateCourseDTO } from '../../DTO/courses/create-course.dto';
import { UpdateCourseDTO } from '../../DTO/courses/update-course.dto';

@Injectable({ providedIn: 'root' })
export class CoursesService extends ResourceState<CourseListDTO, CourseDetailDTO> {
  private readonly api = inject(CoursesApiService);

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

  create(request: CreateCourseDTO) {
    return this.write(this.api.create(request), 'Could not create this course.');
  }

  update(id: string, request: UpdateCourseDTO) {
    return this.write(this.api.update(id, request), 'Could not update this course.');
  }

  remove(id: string, version: string) {
    return this.write(this.api.remove(id, version), 'Could not delete this course.');
  }
}
