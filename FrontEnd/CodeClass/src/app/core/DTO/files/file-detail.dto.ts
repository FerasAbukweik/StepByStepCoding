import { ProgrammingLanguage } from '../../enums/programming-language';

export interface FileDetailDTO {
  id: string;
  version: string;
  lectureId: string | null;
  userId: string | null;
  fileName: string;
  language: ProgrammingLanguage;
  savedContent: string;
  savedRevision: number;
}
