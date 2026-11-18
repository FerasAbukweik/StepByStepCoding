import { AttemptStatus } from '../../enums/attempt-status';

export interface ExamAttemptListDTO {
  id: string;
  version: string;
  examId: string;
  studentId: string;
  studentName: string;
  startedAt: string;
  expiresAt: string;
  status: AttemptStatus;
}
