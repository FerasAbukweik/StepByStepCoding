import { ExamStatus } from '../../enums/exam-status';

export interface ExamDetailDTO {
  instructorId: string;
  id: string;
  version: string;
  sectionId: string;
  title: string;
  instructions: string | null;
  startsAt: string;
  deadline: string;
  durationMinutes: number;
  status: ExamStatus;
}
