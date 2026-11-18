import { EnrollmentStatus } from '../../enums/enrollment-status';

export interface EnrollmentListDTO {
  id: string;
  version: string;
  sectionId: string;
  studentId: string;
  studentName: string;
  status: EnrollmentStatus;
}
