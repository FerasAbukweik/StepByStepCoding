export interface CreateLectureDTO {
  fileName?: string;
  language?: number;
  initialCode?: string;
  sectionId: string;
  title: string;
  scheduledStart: string;
}
