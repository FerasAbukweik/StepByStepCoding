import { EnrollmentStatus } from '../../enums/enrollment-status';

export interface EnrollmentDetailDTO {
  id: string;
  version: string;
  sectionId: string;
  studentId: string;
  status: EnrollmentStatus;
}
