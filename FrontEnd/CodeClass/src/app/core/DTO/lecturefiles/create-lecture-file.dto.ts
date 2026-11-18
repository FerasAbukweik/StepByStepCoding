import { ProgrammingLanguage } from '../../enums/programming-language';

export interface CreateLectureFileDTO {
  lectureId: string;
  fileName: string;
  language: ProgrammingLanguage;
  savedContent: string;
  savedRevision: number;
}
