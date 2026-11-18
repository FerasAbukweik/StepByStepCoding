import { AttemptStatus } from '../../enums/attempt-status';

export interface ExamAttemptDetailDTO {
  id: string;
  version: string;
  examId: string;
  studentId: string;
  startedAt: string;
  expiresAt: string;
  status: AttemptStatus;
}
