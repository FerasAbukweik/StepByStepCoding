import { CodeChangeSet } from '../../../core/DTO/realtime/code-change-set';

export interface EditorChange {
  code: string;
  changeSet: CodeChangeSet;
}
