import { ProgrammingLanguage } from '../../enums/programming-language';

export interface CodeSnapshotDTO {
  lectureId: string;
  fileId: string;
  revision: number;
  code: string;
  fileName: string;
  fileType: ProgrammingLanguage;
}
