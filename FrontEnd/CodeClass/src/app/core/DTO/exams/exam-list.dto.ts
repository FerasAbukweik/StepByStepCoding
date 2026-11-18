import { ExamStatus } from '../../enums/exam-status';

export interface ExamListDTO {
  id: string;
  version: string;
  sectionId: string;
  title: string;
  startsAt: string;
  deadline: string;
  durationMinutes: number;
  status: ExamStatus;
}
