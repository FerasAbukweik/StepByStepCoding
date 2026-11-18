import { ProgrammingLanguage } from '../../enums/programming-language';
import { CodeChangeSet } from './code-change-set';

export interface CodeChangesDTO {
  fileName?: string | null;
  language?: ProgrammingLanguage | null;
  fileId: string;
  lectureId: string | null;
  batchId: string;
  baseRevision: number;
  revision: number;
  changeSets: CodeChangeSet[];
}
