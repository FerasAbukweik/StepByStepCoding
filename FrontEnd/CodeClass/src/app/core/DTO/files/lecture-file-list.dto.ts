import { ProgrammingLanguage } from '../../enums/programming-language';

export interface LectureFileListDTO {
  id: string;
  version: string;
  lectureId: string;
  fileName: string;
  language: ProgrammingLanguage;
  savedRevision: number;
}
