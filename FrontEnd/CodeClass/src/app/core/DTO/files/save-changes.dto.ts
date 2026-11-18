import { ProgrammingLanguage } from '../../enums/programming-language';
import { CodeChangeSet } from '../realtime/code-change-set';

export interface SaveChangesDTO {
  fileName?: string;
  language?: ProgrammingLanguage;
  batchId: string;
  baseRevision: number;
  changeSets: CodeChangeSet[];
}
