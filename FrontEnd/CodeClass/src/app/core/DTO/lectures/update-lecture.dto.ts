export interface UpdateLectureDTO {
  fileId?: string;
  fileVersion?: string;
  fileName?: string;
  language?: number;
  initialCode?: string;
  version: string;
  title: string;
  scheduledStart: string;
}
