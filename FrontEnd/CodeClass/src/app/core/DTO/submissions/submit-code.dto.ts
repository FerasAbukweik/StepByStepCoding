import { ProgrammingLanguage } from '../../enums/programming-language';

export interface SubmitCodeDTO {
  attemptId: string;
  questionId: string;
  submittedCode: string;
  language: ProgrammingLanguage;
}
