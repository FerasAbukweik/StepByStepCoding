import { LectureStatus } from '../../enums/lecture-status';

export interface LectureListDTO {
  id: string;
  version: string;
  sectionId: string;
  fileId: string | null;
  title: string;
  scheduledStart: string;
  startedAt: string | null;
  endedAt: string | null;
  status: LectureStatus;
}
