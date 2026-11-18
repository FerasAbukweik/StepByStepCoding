import { ProgrammingLanguage } from '../../enums/programming-language';

export interface UpdateLectureFileDTO {
  version: string;
  fileName: string;
  language: ProgrammingLanguage;
  savedContent: string;
  savedRevision: number;
}
