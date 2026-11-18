import { Injectable, inject } from '@angular/core';
import { EnrollmentsApiService } from '../api/enrollments-api-service';
import { ResourceState } from '../../Utilities/resource-state';
import { EnrollmentListDTO } from '../../DTO/enrollments/enrollment-list.dto';
import { EnrollmentDetailDTO } from '../../DTO/enrollments/enrollment-detail.dto';

@Injectable({ providedIn: 'root' })
export class EnrollmentsService extends ResourceState<EnrollmentListDTO, EnrollmentDetailDTO> {
  private readonly api = inject(EnrollmentsApiService);

  list(sectionId: string, page = 1) {
    return this.listResult((filters) => this.api.list(filters.sectionId, page), page, {
      sectionId,
    });
  }

  load(sectionId: string, page = 1) {
    return this.loadPage(this.list(sectionId, page), page);
  }

  create(sectionId: string, studentId: string) {
    return this.write(this.api.create(sectionId, studentId), 'Could not enroll this student.');
  }

  update(id: string, version: string, active: boolean) {
    return this.write(this.api.update(id, version, active), 'Could not update this enrollment.');
  }

  remove(id: string, version: string) {
    return this.write(this.api.remove(id, version), 'Could not remove this enrollment.');
  }
}
